using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace Telemetry.Api.DelegatingHandlers
{
	public class GZipToJsonHandler : DelegatingHandler
	{
		public GZipToJsonHandler(HttpConfiguration config)
		{
			this.InnerHandler=new HttpControllerDispatcher(config);
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (!request.IsContentGZip())
			{
				return await base.SendAsync(request, cancellationToken);
			}

			var output = new MemoryStream();
			await request.Content.ReadAsStreamAsync().ContinueWith(t =>
			{
				var input = t.Result;
				using (var gzipStream = new GZipStream(input, CompressionMode.Decompress))
				{
					gzipStream.CopyTo(output);
					output.Flush();
				}
				output.Position = 0;
			}, cancellationToken);

			request.Content = new StreamContent(output);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			return await base.SendAsync(request, cancellationToken);
		}
	}
}
