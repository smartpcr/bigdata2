namespace Telemetry.EventProcessor.DeepStorage.Worker.EventStores
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using Microsoft.WindowsAzure.Storage;
	using Microsoft.WindowsAzure.Storage.Blob;
	using Telemetry.Core;
	using Telemetry.Core.Infrastructure.BlobStorage;

	public class BlobStorageEventStore : EventStoreBase
	{
		private string _blobPath;
		private CloudBlockBlob _blob;
		private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
		private readonly List<string> _blockIds = new List<string>();

		public override int Level { get { return 3; } }

		public override void Initialize(string partitionId, string receivedAtHour)
		{
			base.Initialize(partitionId, receivedAtHour);
			_blobPath = string.Format("{0}/{1}/{2}/p{3}/{4}.json.gz",
				receivedAtHour.Substring(0, 4),
				receivedAtHour.Substring(0, 2),
				receivedAtHour.Substring(6, 2),
				partitionId, receivedAtHour);
			CreateBlob();
		}

		public override void Write(byte[] value)
		{
			try
			{
				_lock.Wait();
				using (var lease = new RenewingBlobLease(_blob))
				{
					var offset = 0;
					while (offset < value.Length)
					{
						var blockId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
						var remaining = value.Length - offset;
						var length = remaining < MaxBufferSize ? remaining : MaxBufferSize;
						using (var stream = new MemoryStream(value, offset, length))
						{
							_blob.PutBlock(blockId, stream, null, AccessCondition.GenerateLeaseCondition(lease.Id));
							offset += (int) stream.Length;
						}
						_blockIds.Add(blockId);
					}
					Flush(lease);
				}
			}
			finally
			{
				_lock.Release();
			}

			AfterWrite();
		}

		private void Flush(RenewingBlobLease lease)
		{
			if (_blockIds.Any())
			{
				var blockList = _blob.DownloadBlockList(BlockListingFilter.Committed,
					AccessCondition.GenerateLeaseCondition(lease.Id));
				var blockIds = blockList.Select(x => x.Name).ToList();
				_blockIds.AddRange(blockIds);
				_blob.PutBlockList(blockIds, AccessCondition.GenerateLeaseCondition(lease.Id));
				_blockIds.Clear();
			}
		}

		public override void Flush()
		{
			try
			{
				_lock.Wait();
				using (var lease = new RenewingBlobLease(_blob))
				{
					Flush(lease);
				}
			}
			finally
			{
				_lock.Release();
			}

			AfterFlush();
		}

		private void CreateBlob()
		{
			var account = CloudStorageAccount.Parse(Config.Get("DeepStorage.OutputConnectionString"));
			var blobClient = account.CreateCloudBlobClient();
			var container = blobClient.GetContainerReference(Config.Get("DeepStorage.OutputContainerName"));
			container.CreateIfNotExists();
			_blob = container.GetBlockBlobReference(_blobPath);
			if (!_blob.Exists())
			{
				_blob.UploadFromByteArray(new byte[] {},0, 0);
			}
		}
	}
}
