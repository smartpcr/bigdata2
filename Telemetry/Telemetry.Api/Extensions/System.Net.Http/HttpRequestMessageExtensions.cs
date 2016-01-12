using System.Collections.Generic;
using System.Linq;

namespace System.Net.Http
{
	public static class HttpRequestMessageExtensions
	{
		public static string GetDeviceId(this HttpRequestMessage request)
		{
			return request.GetHeaderValue("x-device-id");
		}

		public static string GetHeaderValue(this HttpRequestMessage request, string headerName)
		{
			IEnumerable<string> headerValues;
			request.Headers.TryGetValues(headerName, out headerValues);
			if (headerValues != null)
			{
				var value = headerValues.FirstOrDefault();
				return value == null ? string.Empty : value.Trim();
			}
			return string.Empty;
		}

		public static bool IsContentGZip(this HttpRequestMessage request)
		{
			return request.Content.Headers.ContentEncoding.Contains("gzip") ||
				  (request.Content.Headers.ContentType != null &&
					request.Content.Headers.ContentType.MediaType == "application/gzip");
		}

		public static string GetRouteValue(
			this HttpRequestMessage request,
			string routeValueName,
			string defaultValue = null)
		{
			var httpRouteData = request.GetRouteData();
			if (httpRouteData != null)
			{
				string routeValue = httpRouteData.Values["version"].ToString();
				return string.IsNullOrEmpty(routeValue) ? (defaultValue ?? string.Empty) : routeValue;
			}

			return string.Empty;
		}
	}
}
