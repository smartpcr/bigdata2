using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Telemetry.Api
{
    using Telemetry.Core.Logging;

    public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			LogSetup.Run();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}
	}
}
