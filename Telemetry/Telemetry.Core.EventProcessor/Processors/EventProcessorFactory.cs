using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Core.EventProcessor.Processors
{
    using Microsoft.ServiceBus.Messaging;
    public class EventProcessorFactory : IEventProcessorFactory
    {
        private readonly IEventProcessor _processor;

        public EventProcessorFactory(IEventProcessor processor)
        {
            _processor = processor;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return _processor;
        }
    }
}
