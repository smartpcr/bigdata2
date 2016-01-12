using NLog;
using Telemetry.Core.Logging;

namespace System
{
	public static class TypeExtensions
	{
		public static string GetRegistrationName(this Type type)
		{
			return type.Namespace + "." + type.Name;
		}

		public static string GetMessageTypeName(this Type type)
		{
			return type.FullName;
		}

		public static Logger GetLogger(this Type type)
		{
			LogSetup.Run();
			return LogManager.GetLogger(type.Name);
		}
	}
}
