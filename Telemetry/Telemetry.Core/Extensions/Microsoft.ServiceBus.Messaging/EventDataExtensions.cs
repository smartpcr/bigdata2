using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ServiceBus.Messaging
{
	public static class EventDataExtensions
	{
		public static string GetEventName(this EventData eventData)
		{
			return (string)GetPropertyValue(eventData, "eventName");
		}

		public static void SetEventName(this EventData eventData, string eventName)
		{
			SetPropertyValue(eventData, "eventName", eventName);
		}

		public static long GetReceivedAt(this EventData eventData)
		{
			return (long)GetPropertyValue(eventData, "receivedAt", long.MinValue);
		}

		public static string GetReceivedAtHour(this EventData eventData)
		{
			var receivedAt = eventData.GetReceivedAt();
			var receivedDateTime = receivedAt > long.MinValue ? DateTimeExtensions.FromUnixMilliseconds(receivedAt) : DateTime.UtcNow;
			return receivedDateTime.ToString("yyyyMMddHH");
		}

		public static void SetReceivedAt(this EventData eventData, long receivedAt)
		{
			SetPropertyValue(eventData, "receivedAt", receivedAt);
		}

		private static object GetPropertyValue(EventData eventData, string propertyName, object defaultValue = null)
		{
			object value = defaultValue;
			if (eventData.Properties != null && eventData.Properties.ContainsKey(propertyName))
			{
				value = eventData.Properties[propertyName];
			}
			return value;
		}

		private static void SetPropertyValue(EventData eventData, string propertyName, object value)
		{
			eventData.Properties[propertyName] = value;
		}
	}
}
