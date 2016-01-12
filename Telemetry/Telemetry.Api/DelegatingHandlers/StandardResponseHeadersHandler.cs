using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Telemetry.Api.DelegatingHandlers
{
	public class StandardResponseHeadersHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			return await base.SendAsync(request, cancellationToken).ContinueWith(task =>
			{
				task.Result.AddStandardHeaders();
				return task.Result;
			}, cancellationToken);
		}
	}
}
