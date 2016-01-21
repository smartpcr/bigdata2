namespace Telemetry.Api.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using Newtonsoft.Json;
    using Telemetry.Core.Extensions.NLog;
    using Telemetry.Core.Logging;

    public class DebugActionWebApiFilter : ActionFilterAttribute
    {
        private Stopwatch watch;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            watch = new Stopwatch();
            watch.Start();
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            watch.Stop();
            var logger = actionExecutedContext.ActionContext.ControllerContext.Controller.GetLogger();
            var controllerName = actionExecutedContext.ActionContext.ControllerContext.Controller.GetType().Name;
            var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            var args = actionExecutedContext.ActionContext.ActionArguments;
            string msg = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}: OnActionExecuted", DateTime.UtcNow);
            List<Facet> facets = new List<Facet>();
            facets.Add(new Facet("Controller", controllerName));
            facets.Add(new Facet("Action", actionName));
            facets.Add(new Facet("Span", watch.Elapsed));
            if (args != null)
            {
                facets.Add(new Facet("Args", JsonConvert.SerializeObject(args)));
            }
            logger.InfoEvent(msg, facets.ToArray());
        }
    }
}
