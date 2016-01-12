using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using Telemetry.Api.Analytics.Spec;
using Telemetry.Core.Extensions.NLog;
using Telemetry.Core.Logging;

namespace Telemetry.Api.Analytics.IngestionApi
{
	public class IngestionApiEventSender : IEventSender
	{
		private Logger _log;
		private readonly string _requestUrl;

		public IngestionApiEventSender(string requestUrl)
		{
			_log = this.GetLogger();
			_requestUrl = requestUrl;
		}

		public async Task SendEventsAsync(JArray events, string deviceId)
		{
			_log.TraceEvent("SendEvents",
				new Facet("eventCount", events.Count));

			if (events.Count > 0)
			{
				using (var client = new HttpClient())
				{
					var eventJson = events.ToString();
					var requestContent = new StringContent(eventJson);
					requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
					await client.PostAsync(_requestUrl, requestContent);
				}
			}
		}
	}
}
