using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;
using Telemetry.Core.Logging;

namespace Telemetry.Core.Extensions.NLog
{
	public static class LoggerExtensions
	{
		private static JsonSerializerSettings _JsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			NullValueHandling = NullValueHandling.Ignore
		};

		static LoggerExtensions()
		{
			_JsonSettings.Converters.Add(new StringEnumConverter());
		}

		public static void TraceEvent(this Logger log, string name, params Facet[] facets)
		{
			LogEvent(log, l => l.IsTraceEnabled, (l, e) => l.Trace(e), name, facets);
		}

		public static void DebugEvent(this Logger log, string name, params Facet[] facets)
		{
			LogEvent(log, l => l.IsDebugEnabled, (l, e) => l.Debug(e), name, facets);
		}

		public static void InfoEvent(this Logger log, string name, params Facet[] facets)
		{
			LogEvent(log, l => l.IsInfoEnabled, (l, e) => l.Info(e), name, facets);
		}

		public static void WarnEvent(this Logger log, string name, params Facet[] facets)
		{
			LogEvent(log, l => l.IsWarnEnabled, (l, e) => l.Warn(e), name, facets);
		}

		public static Guid ErrorEvent(this Logger log, string name, Exception ex, params Facet[] facets)
		{
			var facetList = new List<Facet>(facets) { new Facet { Name = "_exception", Value = ex.ToString() } };
			var errorId = Guid.NewGuid();
			facetList.Add(new Facet
			{
				Name = "_errorId",
				Value = errorId.ToString()
			});
			LogEvent(log, l => l.IsErrorEnabled, (l, e) => l.Error(e), name, facetList.ToArray());
			return errorId;
		}

		private static void LogEvent(Logger log, Func<Logger, bool> isEnabled, Action<Logger, string> logAction, string name, params Facet[] facets)
		{
			var loggingEnabled = Config.Parse<bool>("LoggingEnabled");
			if (loggingEnabled && isEnabled(log))
			{
				var facetList = new List<Facet>(facets)
				{
					new Facet { Name = "_loggerId", Value = log.GetHashCode() }
				};
				var @event = new Event(name, facetList);
				var json = JsonConvert.SerializeObject(@event, _JsonSettings);
				logAction(log, json);
			}
		}
	}
}
