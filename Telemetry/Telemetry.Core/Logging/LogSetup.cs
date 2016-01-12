using System;
using System.IO;
using NLog;
using NLog.Config;

namespace Telemetry.Core.Logging
{
	public static class LogSetup
	{
		private static bool _hasRun;

		public static void Run()
		{
			if (!_hasRun)
			{
				var environment = Config.Get("Environment");
				var configFileName = string.Format("nlog-{0}.config", environment);
				var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
				if (File.Exists(configFilePath))
				{
					LogManager.Configuration = new XmlLoggingConfiguration(configFilePath, true);
				}
				_hasRun = true;
			}
		}
	}
}
