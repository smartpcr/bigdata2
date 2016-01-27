namespace Telemetry.Api
{
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using Microsoft.Owin.Security.OAuth;
    using Telemetry.Api.DelegatingHandlers;
    using Telemetry.Api.Filters;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());
            
			config.Filters.Add(new LoggingExceptionFilterAttribute());
			config.MapHttpAttributeRoutes();
			config.MessageHandlers.Add(new StandardResponseHeadersHandler());
		}
    }
}
