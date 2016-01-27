using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Core.EventProcessor
{
    using Microsoft.ServiceBus.Messaging;

    public class EventReceiver
    {
        private readonly IEventProcessorFactory _processorFactory;
        private readonly EventProcessorHost _host;

        public EventReceiver(IEventProcessorFactory processorFactory, string eventHubName, string consumerGroupName, 
            string eventHubConnectionString, string checkpointConnectionString)
        {
            _processorFactory = processorFactory;
            _host = new EventProcessorHost(Environment.MachineName, eventHubName, consumerGroupName,
                eventHubConnectionString, checkpointConnectionString);
        }

        public async Task RegisterProcessorAsync()
        {
            var processorOps = new EventProcessorOptions()
            {
                MaxBatchSize=5000,
                PrefetchCount=1000
            };
            await _host.RegisterEventProcessorFactoryAsync(_processorFactory, processorOps);
        }

        public async Task UnregisterProcessorAsync()
        {
            if (_host != null)
            {
                await _host.UnregisterEventProcessorAsync();
            }
        }
    }
}
