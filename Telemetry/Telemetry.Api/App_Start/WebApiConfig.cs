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

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			config.Filters.Add(new LoggingExceptionFilterAttribute());
			// Web API routes
			config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

			config.MessageHandlers.Add(new StandardResponseHeadersHandler());
		}
    }
}
