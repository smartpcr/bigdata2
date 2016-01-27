using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetry.Core.EventProcessor.Spec
{
    using Microsoft.ServiceBus.Messaging;

    public interface IEventHandler : IDisposable
    {
        bool IsHandled(string eventName);
        void Handle(EventData eventData, string partitionId);
    }
}
