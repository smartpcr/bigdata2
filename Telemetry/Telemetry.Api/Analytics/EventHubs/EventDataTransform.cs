using System.Text;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Telemetry.Api.Analytics.EventHubs
{
	public static class EventDataTransform
	{
		public static EventData ToEventData(dynamic eventObject, out int payloadSize)
		{
			var json = eventObject.ToString(Formatting.None);
			payloadSize = Encoding.UTF8.GetByteCount(json);
			var payload = Encoding.UTF8.GetBytes(json);
			var eventData = new EventData(payload)
			{
				PartitionKey = (string)eventObject.deviceId
			};
			eventData.SetEventName((string)eventObject.eventName);
			eventData.SetReceivedAt((long)eventObject.receivedAt);
			return eventData;
		}
	}
}
