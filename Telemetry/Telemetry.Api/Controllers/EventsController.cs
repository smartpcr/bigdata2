using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Telemetry.Api.Analytics;
using Telemetry.Api.Analytics.Spec;
using Telemetry.Core;
using Telemetry.Core.Extensions.NLog;
using Telemetry.Core.Logging;

namespace Telemetry.Api.Controllers
{
    public class EventsController : ApiController
    {
		private readonly Logger _log;
		private readonly IEventSender _sender;

		public EventsController(IEventSender sender)
		{
			_log = this.GetLogger();
			_sender = sender;
		}

		public async Task<HttpResponseMessage> Post(HttpRequestMessage requestMessage)
		{
			var deviceId = Request.GetDeviceId();
			var json = string.Empty;
			JArray events;

			try
			{
				json = await requestMessage.Content.ReadAsStringAsync();
				dynamic request = JObject.Parse(json);
				events = (JArray)request.events;

				_log.TraceEvent("ParseEvents",
					new Facet("deviceId", deviceId),
					new Facet("eventCount", events.Count));
			}
			catch (Exception ex)
			{
				var errorId = _log.ErrorEvent("ParseEvents", ex,
					new Facet("json", json));

				var error = new { errorId = errorId };
				var errorJson = JsonConvert.SerializeObject(error);

				return new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = new StringContent(errorJson)
				};
			}

			if (Config.Parse<bool>("Telemetry.DeviceEvents.SendToEventHubs"))
			{
				try
				{
					await _sender.SendEventsAsync(events, deviceId);
					return new HttpResponseMessage(HttpStatusCode.Created);
				}
				catch (Exception ex)
				{
					var errorId = _log.ErrorEvent("SendEvents", ex);
					var error = new { errorId = errorId };
					var errorJson = JsonConvert.SerializeObject(error);

					return new HttpResponseMessage(HttpStatusCode.InternalServerError)
					{
						Content = new StringContent(errorJson)
					};
				}
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}
	}
}
