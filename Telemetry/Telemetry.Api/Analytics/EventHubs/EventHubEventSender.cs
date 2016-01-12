using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using NLog;
using Telemetry.Api.Analytics.Spec;
using Telemetry.Core;
using Telemetry.Core.Extensions.NLog;
using Telemetry.Core.Logging;

namespace Telemetry.Api.Analytics.EventHubs
{
	public class EventHubEventSender : IEventSender
	{
		private Logger _log;

		public EventHubEventSender()
		{
			_log = this.GetLogger();
		}

		public async Task SendEventsAsync(JArray events, string deviceId)
		{
			if (events.Count > 0)
			{
				var connectionString = Config.Get("Telemetry.DeviceEvents.ConnectionString");
				var eventHubName = Config.Get("Telemetry.DeviceEvents.EventHubName");
				var client = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);

				var iterator = new EventBatchIterator(events);
				foreach (var batch in iterator)
				{
					try
					{
						await client.SendBatchAsync(batch);

						_log.TraceEvent("SendEventsAsync",
							new Facet("deviceId", deviceId),
							new Facet("batchCount", batch.Count()));
					}
					catch (Exception ex)
					{
						_log.ErrorEvent("SendEventsAsync", ex,
							new Facet("deviceId", deviceId),
							new Facet("batchCount", batch.Count()));

						throw;
					}
				}
			}
		}
	}
}
