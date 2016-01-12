using System.Net.Http.Headers;
using Telemetry.Api;

namespace System.Net.Http
{
	public static class HttpResponseMessageExtensions
	{
		static readonly MediaTypeHeaderValue _MediaType;
		static readonly string _VersionNumber;
		static readonly string _BuildNumber;

		static HttpResponseMessageExtensions()
		{
			_MediaType = new MediaTypeHeaderValue("application/json");
			var version = typeof(WebApiApplication).Assembly.GetName().Version;
			//build number is the full version number, e.g. 1.12.345.1
			_BuildNumber = version.ToString();
			//API version is the major & minor, e.g. 1.12:
			_VersionNumber = string.Format("{0}.{1}", version.Major, version.Minor);
		}

		public static void AddStandardHeaders(this HttpResponseMessage message)
		{
			AddResponseHeaders(message.Headers);
			if (message.Content != null)
			{
				AddContentHeaders(message.Content.Headers);
			}
		}

		private static void AddResponseHeaders(HttpResponseHeaders headers)
		{
			headers.Add("x-api-version", _VersionNumber);
			headers.Add("x-api-build", _BuildNumber);
		}

		private static void AddContentHeaders(HttpContentHeaders headers)
		{
			headers.ContentType = _MediaType;
		}
	}
}
