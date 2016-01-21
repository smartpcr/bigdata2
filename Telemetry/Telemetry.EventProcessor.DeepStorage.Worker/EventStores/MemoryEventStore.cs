namespace Telemetry.EventProcessor.DeepStorage.Worker.EventStores
{
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	public class MemoryEventStore : EventStoreBase
	{
		private StringBuilder _buffer;
		private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

		public override int Level { get { return 1; } }

		public override void Initialize(string partitionId, string receivedAtHour)
		{
			base.Initialize(partitionId, receivedAtHour);
			_buffer = new StringBuilder(MaxBufferSize);
		}

		public override void Write(byte[] value)
		{
			var byteCount = value.Length;
			var json = Encoding.UTF8.GetString(value);

			if (_buffer.Length + byteCount > MaxBufferSize)
			{
				Flush();
			}

			try
			{
				_lock.Wait();
				_buffer.AppendLine(json);
			}
			finally
			{
				_lock.Release();
			}

			AfterWrite();
		}

		public override void Flush()
		{
			var block = string.Empty;

			try
			{
				_lock.Wait();
				block = _buffer.ToString();
				_buffer.Clear();
			}
			finally
			{
				_lock.Release();
			}

			if (block.Length > 0)
			{
				var data = Encoding.UTF8.GetBytes(block);
				Task.Factory.StartNew(() => NextStore.Write(data));
			}

			AfterFlush();
		}
	}
}
