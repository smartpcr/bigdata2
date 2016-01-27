using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Core.EventProcessor.Processors
{
    using System.ServiceModel.Dispatcher;
    using Microsoft.ServiceBus.Messaging;
    using NLog;
    using Telemetry.Core.EventProcessor.Spec;
    using Telemetry.Core.Extensions.NLog;
    using Telemetry.Core.Logging;

    public class HandledEventProcessor : IEventProcessor
    {
        protected Logger _logger;
        private readonly EventHandlerFactory _handlerFactory;

        public HandledEventProcessor(EventHandlerFactory handlerFactory)
        {
            _logger = this.GetLogger();
            _handlerFactory = handlerFactory;
        }

        public async Task OpenAsync(PartitionContext context)
        {
            _logger.InfoEvent("Open",
                new Facet("eventHubPath", context.EventHubPath),
                new Facet("partitionId", context.Lease.PartitionId),
                new Facet("offset", context.Lease.Offset));
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            try
            {
                foreach (EventData eventData in messages)
                {
                    var eventName = eventData.GetEventName();
                    var handlers = _handlerFactory.GetHandlers(eventName);
                    if (handlers.Any())
                    {
                        foreach (var handler in handlers)
                        {
                            SafelyHandleEvent(handler, eventName, eventData, context);
                        }
                    }
                    else
                    {
                        _logger.WarnEvent("NoEventHandlers", new Facet("eventName", eventName));
                    }
                }

                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                _logger.ErrorEvent("ProcessEventFailed", ex,
                    new Facet("eventCount", messages.Count()),
                    new Facet("eventHubPath", context.EventHubPath),
                    new Facet("partitionId", context.Lease.PartitionId));
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            _logger.InfoEvent("Close",
                   new Facet { Name = "reason", Value = reason },
                   new Facet { Name = "partitionId", Value = context.Lease.PartitionId },
                   new Facet { Name = "offset", Value = context.Lease.Offset });

            foreach (var handler in _handlerFactory.Handlers)
            {
                handler.Dispose();
            }
        }

        private void SafelyHandleEvent(IEventHandler handler, string EventDataSystemPropertyNames, EventData eventData,
            PartitionContext context)
        {
            try
            {
                handler.Handle(eventData.Clone(), context.Lease.PartitionId);
            }
            catch (Exception ex)
            {
                _logger.ErrorEvent("HandleEventFailed", ex,
                    new Facet("handlerFullName", handler.GetType().FullName),
                    new Facet("eventHubPath", context.EventHubPath),
                    new Facet("partitionId", context.Lease.PartitionId));
            }
        }
    }
}
