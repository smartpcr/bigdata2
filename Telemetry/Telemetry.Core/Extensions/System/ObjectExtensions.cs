using NLog;

namespace System
{
	public static class ObjectExtensions
	{

		public static string GetMessageTypeName(this object obj)
		{
			return obj.GetType().GetMessageTypeName();
		}

		public static Logger GetLogger(this object obj)
		{
			return obj.GetType().GetLogger();
		}
	}
}
