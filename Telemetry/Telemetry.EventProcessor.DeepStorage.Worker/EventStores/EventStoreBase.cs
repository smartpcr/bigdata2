namespace Telemetry.EventProcessor.DeepStorage.Worker.EventStores
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.Practices.Unity;
	using Telemetry.Core;

	public abstract class EventStoreBase : IEventStore
	{
		private Stopwatch _flushStopwatch;
		protected List<Task> _startedTasks = new List<Task>();

		public abstract int Level { get; }
		public string ReceiveAtHour { get; private set; }
		public string PartitionId { get; private set; }
		public IEventStore NextStore { get; private set; }

		public int MaxBufferSize
		{
			get { return Config.Parse<int>("EventStores." + GetType().Name + ".MaxBufferSize"); }
		}
		public TimeSpan? MaxBufferTime
		{
			get { return Config.Parse<TimeSpan>("EventStores.OverdueFlushPeriod"); }
		}

		public virtual void Initialize(string partitionId, string receivedAtHour)
		{
			PartitionId = partitionId;
			ReceiveAtHour = receivedAtHour;

			if (MaxBufferTime.HasValue)
			{
				_flushStopwatch = Stopwatch.StartNew();
			}

			var nextLevel = (Level + 1).ToString();
			try
			{
				NextStore = Container.Instance.Resolve<IEventStore>(nextLevel);
				if (NextStore != null)
				{
					NextStore.Initialize(partitionId, receivedAtHour);
				}
			}
			catch { }
		}

		public abstract void Write(byte[] value);

		public abstract void Flush();

		public virtual void AfterFlush()
		{
			_flushStopwatch = Stopwatch.StartNew();
		}

		protected void AfterWrite()
		{
			if (IsFlushOverdue())
			{
				Flush();
			}
		}

		public bool IsFlushOverdue()
		{
			return MaxBufferTime.HasValue &&
			       _flushStopwatch != null &&
			       _flushStopwatch.ElapsedMilliseconds > MaxBufferTime.Value.TotalMilliseconds;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();

				if (_startedTasks.Any())
				{
					Task.WaitAll(_startedTasks.ToArray());
				}
			}
			if (NextStore != null)
			{
				NextStore.Dispose();
			}
		}
	}
}
