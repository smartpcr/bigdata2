using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Telemetry.Api.Analytics
{
	public interface IEventSender
	{
		Task SendEventsAsync(JArray events, string deviceId);
	}
}
