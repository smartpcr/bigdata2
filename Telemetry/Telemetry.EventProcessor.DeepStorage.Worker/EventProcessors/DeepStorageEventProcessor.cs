namespace Telemetry.EventProcessor.DeepStorage.Worker.EventProcessors
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Timers;
	using Microsoft.Practices.Unity;
	using Microsoft.ServiceBus.Messaging;
	using NLog;
	using Telemetry.Core;
	using Telemetry.Core.Extensions.NLog;
	using Telemetry.Core.Logging;
	using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;

	public class DeepStorageEventProcessor : IEventProcessor
	{
		private readonly Logger _log;
		private static ConcurrentDictionary<string, IEventStore> _eventStores = new ConcurrentDictionary<string, IEventStore>();
		private static Timer _storeFlushTimer;

		static DeepStorageEventProcessor()
		{
			var overdueStoreFlushTime = Config.Parse<TimeSpan>("EventStores.OverdueFlushPeriod");
			_storeFlushTimer = new Timer(overdueStoreFlushTime.TotalMilliseconds);
			_storeFlushTimer.Elapsed += FlushOverdueStores;
			_storeFlushTimer.Start();
		}

		public DeepStorageEventProcessor()
		{
			_log = this.GetLogger();
		}

		public async Task OpenAsync(PartitionContext context)
		{
			_log.InfoEvent("Open",
				new Facet("eventHubPath", context.EventHubPath),
				new Facet("partitionId", context.Lease.PartitionId),
				new Facet("offset", context.Lease.Offset));
		}

		public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
		{
			_log.DebugEvent("HandlesEvent",
				new Facet("state", "Received"),
				new Facet("eventCount", messages.Count()),
				new Facet("eventHubPath", context.EventHubPath),
				new Facet("partitionId", context.Lease.PartitionId));

			foreach (var eventData in messages)
			{
				var store = GetEventStore(eventData, context.Lease.PartitionId);
				var bytes = eventData.GetBytes();
				store.Write(bytes);
			}

			await context.CheckpointAsync();
		}

		public async Task CloseAsync(PartitionContext context, CloseReason reason)
		{
			_log.InfoEvent("Close",
				new Facet("reason", reason),
				new Facet("partitionId", context.Lease.PartitionId),
				new Facet("offset", context.Lease.Offset));
		}

		private IEventStore GetEventStore(EventData eventData, string partitionId)
		{
			var receivedAt = eventData.GetReceivedAtHour();
			var key = string.Format("{0}p{1}", receivedAt, partitionId);
			if (!_eventStores.ContainsKey(key))
			{
				var store = Container.Instance.Resolve<IEventStore>("1");
				store.Initialize(partitionId, receivedAt);
				_eventStores[key] = store;
			}

			return _eventStores[key];
		}

		private static void FlushOverdueStores(object sender, ElapsedEventArgs e)
		{
			FlushOverdueStores();
		}

		public static void FlushOverdueStores()
		{
			var overdueStoreKeys = new List<string>();
			var beforeCount = _eventStores.Count;
			foreach (var store in _eventStores)
			{
				if (store.Value.IsFlushOverdue())
				{
					overdueStoreKeys.Add(store.Key);
				}
			}

			foreach (var overdueStoreKey in overdueStoreKeys)
			{
				IEventStore overdueStore;
				if (_eventStores.TryRemove(overdueStoreKey, out overdueStore))
				{
					overdueStore.Flush();
					overdueStore.Dispose();
				}
			}

			var afterCount = _eventStores.Count;
			// todo: count logging
		}
	}
}
