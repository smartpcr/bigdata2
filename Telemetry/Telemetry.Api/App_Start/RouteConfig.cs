using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Telemetry.Api.DelegatingHandlers;

namespace Telemetry.Api
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
//			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
//
//			routes.MapRoute(
//				name: "Default",
//				url: "{controller}/{action}/{id}",
//				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
//			);

			routes.MapHttpRoute(
				name: "device-events",
				routeTemplate: "events",
				defaults: new { controller = "Events" },
				handler: new GZipToJsonHandler(GlobalConfiguration.Configuration),
				constraints: null);
		}
	}
}
