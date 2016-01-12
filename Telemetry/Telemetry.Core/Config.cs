using System;
using System.Runtime.Caching;
using Microsoft.WindowsAzure;

namespace Telemetry.Core
{
	public static class Config
	{
		private static MemoryCache _cache;
		private static TimeSpan _cacheSpan;

		static Config()
		{
			_cacheSpan = TimeSpan.Parse(CloudConfigurationManager.GetSetting("ConfigCacheTimespan"));
			_cache = new MemoryCache("Telemetry.Config.Cache");
		}

		public static T Parse<T>(string name) where T : IComparable
		{
			IComparable value = default(T);
			var valueString = Get(name);
			if (valueString != null)
			{
				if (value is TimeSpan)
				{
					value = TimeSpan.Parse(valueString);
				}
				else if (value is int)
				{
					value = int.Parse(valueString);
				}
				else if (value is bool)
				{
					value = bool.Parse(valueString);
				}
			}
			return (T)value;
		}

		public static string Get(string name)
		{
			string item = null;

			if (!string.IsNullOrEmpty(name))
			{
				if (_cache.Contains(name))
				{
					item = (string)_cache[name];
				}
				else
				{
					item = CloudConfigurationManager.GetSetting(name);

					if (_cacheSpan.TotalSeconds > 0 && !string.IsNullOrEmpty(item))
					{
						var cacheItem = new CacheItem(name, item);
						var policy = new CacheItemPolicy
						{
							AbsoluteExpiration = DateTimeOffset.Now.Add(_cacheSpan)
						};
						_cache.Add(cacheItem, policy);
					}
				}
			}

			return item;
		}
	}
}
