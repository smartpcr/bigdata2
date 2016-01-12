namespace System
{
	public static class DateTimeExtensions
	{
		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long ToUnixMillseconds(this DateTime time)
		{
			return (long)(time - Epoch).TotalMilliseconds;
		}

		public static DateTime FromUnixMilliseconds(long milliseconds)
		{
			return Epoch.AddMilliseconds(milliseconds);
		}

		public static string ToHourPeriod(this DateTime dateTime)
		{
			return dateTime.ToString("yyyyMMddHH");
		}

		public static string ToDayPeriod(this DateTime dateTime)
		{
			return dateTime.ToString("yyyyMMdd");
		}

		public static string ToMonthPeriod(this DateTime dateTime)
		{
			return dateTime.ToString("yyyyMM");
		}
	}
}
