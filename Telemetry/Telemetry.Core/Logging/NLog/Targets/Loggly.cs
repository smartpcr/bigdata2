using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Telemetry.Core.Logging.NLog.Targets
{
	//modified from git@github.com:tiernano/LogglyNLoggerTarget.git

	[Target("Loggly")]
	public class Loggly : TargetWithLayout
	{
		private Dictionary<Guid, string> _buffer = new Dictionary<Guid, string>();
		private Timer _flushTimer;
		private TimeSpan _flushTimespan;
		private static object _SyncLock = new object();

		[RequiredParameter]
		public virtual string URL { get; set; }

		protected override void Write(LogEventInfo logEvent)
		{
			if (!string.IsNullOrEmpty(URL))
			{
				var logMessage = this.Layout.Render(logEvent);
				using (var client = new HttpClient())
				{
					var content = new StringContent(logMessage, Encoding.UTF8, "application/x-www-form-urlencoded");
					var postTask = client.PostAsync(URL, content);
					postTask.Wait();
				}
			}
		}
	}
}
