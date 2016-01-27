using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.EventProcessor.DeepStorage.Worker.EventHandlers
{
    using System.Collections.Concurrent;
    using System.Timers;
    using Microsoft.Practices.Unity;
    using Microsoft.ServiceBus.Messaging;
    using NLog;
    using Telemetry.Core;
    using Telemetry.Core.EventProcessor.Spec;
    using Telemetry.EventProcessor.DeepStorage.Worker.EventStores;

    public class DeepStorageEventHandler : IEventHandler
    {
        private readonly Logger _log;
        private static ConcurrentDictionary<string, IEventStore> _eventStores = new ConcurrentDictionary<string, IEventStore>();
        private static Timer _storeFlushTimer;

        static DeepStorageEventHandler()
        {
            var overdueStoreFlushTime = Config.Parse<TimeSpan>("EventStores.OverdueFlushPeriod");
            _storeFlushTimer = new Timer(overdueStoreFlushTime.TotalMilliseconds);
            _storeFlushTimer.Elapsed += FlushOverdueStores;
            _storeFlushTimer.Start();
        }

        public DeepStorageEventHandler()
        {
            _log = this.GetLogger();
        }
        
        public bool IsHandled(string eventName)
        {
            // handle all events
            return true;
        }

        public void Handle(EventData eventData, string partitionId)
        {
            var store = GetEventStore(eventData, partitionId);
            var bytes = eventData.GetBytes();
            store.Write(bytes);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                FlushOverdueStores();
            }
        }
    }
}
