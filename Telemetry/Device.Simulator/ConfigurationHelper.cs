﻿namespace Devices.Simulator
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// Helper class to abstract retrieving configuration properties from .NET or Azure
    /// configuration.
    /// </summary>
    public abstract class ConfigurationHelper
    {
        /// <summary>
        /// Given a configuration key (in either the Azure service configuration or
        /// app/web.config) attempt to retrieve a valid SQL Server connection
        /// string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConnectionString(string key)
        {
            string connectionString = String.Empty;
            if (TryGetAzureConfigValue<string>(key, out connectionString))
            {
                try
                {
                    // Force parsing of the connection string
                    new SqlConnectionStringBuilder(connectionString);
                    return connectionString;
                }
                catch (Exception)
                {
                    throw new ArgumentException("Connection string " + connectionString + " is not valid");
                }
            }

            if (TryGetAppConnectionString(key, out connectionString))
                return connectionString;
            return String.Empty;
        }

        /// <summary>
        /// Try to get conneciton string from the configuraiton appsettngs
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool TryGetAppConnectionString(string key, out string val)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[key];
            if (connectionString != null)
            {
                val = connectionString.ConnectionString;
                return true;
            }

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                try
                {
                    var text = ConfigurationManager.AppSettings[key];
                    var builder = new SqlConnectionStringBuilder(text);
                    val = builder.ConnectionString;
                    return true;
                }
                catch (Exception)
                {
                    // It's okay to fail, just return false
                }
            }

            val = String.Empty;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool TryGetConfigValue<T>(string key, out T val)
        {
            if (TryGetAzureConfigValue<T>(key, out val))
                return true;
            if (TryGetAppConfigValue<T>(key, out val))
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public static T GetConfigValue<T>(string key)
        {
            var ret = default(T);
            if (TryGetAzureConfigValue<T>(key, out ret))
                return ret;
            if (TryGetAppConfigValue<T>(key, out ret))
                return ret;
            throw new ArgumentException("Could not lookup configuration value " + key + " as type " + typeof(T).Name);
        }

        /// <summary>
        /// 
        /// </summary>
        public static T GetConfigValue<T>(string key, T def)
        {
            var ret = default(T);
            if (TryGetAzureConfigValue<T>(key, out ret))
                return ret;
            if (TryGetAppConfigValue<T>(key, out ret))
                return ret;
            return def;
        }

        public static object GetConfigValue(Type type, string key, object def)
        {
            string strValue = GetConfigValue<string>(key,
                (def == null) ? string.Empty : def.ToString());
            object ret = ConvertValue(type, key, strValue);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        internal static T GetAzureConfigValue<T>(string key)
        {
            var ret = default(T);
            if (TryGetAzureConfigValue<T>(key, out ret))
                return ret;
            throw new ArgumentException("Could not lookup Azure configuration value " + key + " as type " + typeof(T).Name);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static T GetAppConfigValue<T>(string key)
        {
            var ret = default(T);
            if (TryGetAppConfigValue<T>(key, out ret))
                return ret;
            throw new ArgumentException("Could not lookup app configuration value " + key + " as type " + typeof(T).Name);
        }

        private static readonly string STORAGE_ACCOUNT = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";

        public static CloudStorageAccount GetDiagnosticsStorage()
        {
            var accountInfo = GetConfigValue<string>(STORAGE_ACCOUNT);
            return CloudStorageAccount.Parse(accountInfo);
        }

        /// <summary>
        /// If the Azure role environment is available, attempt to get the configuration
        /// value from the role configuration as the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool TryGetAzureConfigValue<T>(string key, out T val)
        {
            val = default(T);
            try
            {
                if (!RoleEnvironment.IsAvailable)
                    return false;
                object strValue = RoleEnvironment.GetConfigurationSettingValue(key);
                val = ConvertValue<T>(key, strValue);

                return true;
            }
            catch (RoleEnvironmentException)
            {
                // When the trace destination is being looked up from configuration settings for the
                // azure trace listener _trace may yet be invalid
                System.Diagnostics.Trace.WriteLine(String.Format(CultureInfo.InvariantCulture,
                    "The requested key {0} does not exist in Azure role configuration", key));
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Attempt to get the given configuration value from the .NET (app.config / web.config)
        /// setting as the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool TryGetAppConfigValue<T>(string key, out T val)
        {
            val = default(T);
            try
            {
                if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
                    return false;

                object strValue = ConfigurationManager.AppSettings[key];

                if (typeof(T) == typeof(string))
                    val = (T)strValue;
                else
                {
                    val = (T)ConvertValue<T>(key, strValue);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Attempt to coerce a value to the appropriate type and log the error as appropriate
        /// </summary>    
        public static T ConvertValue<T>(string key, object obj)
        {
            try
            {
                T val = default(T);
                if (typeof(T) == typeof(string))
                    val = (T)obj;
                else if (typeof(T) == typeof(TimeSpan))
                {
                    val = (T)(object)TimeSpan.Parse(obj.ToString(), CultureInfo.InvariantCulture);
                }
                else if (typeof(T) == typeof(Guid))
                {
                    val = (T)(object)Guid.Parse(obj.ToString());
                }
                else if (typeof(T) == typeof(DateTimeOffset))
                {
                    val = (T)(object)DateTimeOffset.Parse(obj.ToString(), null, System.Globalization.DateTimeStyles.AssumeUniversal);
                }
                else if (typeof(T) == typeof(CloudStorageAccount))
                {
                    val = (T)(object)CloudStorageAccount.Parse(obj.ToString());
                }
                else if (typeof(T).IsEnum)
                {
                    val = (T)Enum.Parse(typeof(T), obj.ToString());
                }
                else
                {
                    val = (T)Convert.ChangeType(obj, typeof(T), CultureInfo.InvariantCulture);
                }
                return val;
            }
            catch (Exception ex0)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Could not convert key {0} for value {1} to type {2}: {3}",
                    key, obj, typeof(T).Name, ex0));
                throw;
            }
        }

        /// <summary>
        /// Attempt to coerce a value to the appropriate type and log the error as appropriate
        /// </summary>    
        public static object ConvertValue(Type destType, string key, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (destType == null)
                throw new ArgumentNullException("destType");

            try
            {
                object val = null;
                if (destType == typeof(string))
                    val = obj;
                else if (destType == typeof(TimeSpan))
                {
                    val = TimeSpan.Parse(obj.ToString(), CultureInfo.InvariantCulture);
                }
                else if (destType == typeof(Guid))
                {
                    val = Guid.Parse(obj.ToString());
                }
                else if (destType == typeof(DateTimeOffset))
                {
                    val = DateTimeOffset.Parse(obj.ToString(), null, System.Globalization.DateTimeStyles.AssumeUniversal);
                }
                else if (destType.IsEnum)
                {
                    val = Enum.Parse(destType, obj.ToString());
                }
                else
                {
                    val = Convert.ChangeType(obj, destType, CultureInfo.InvariantCulture);
                }
                return val;
            }
            catch (Exception ex0)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "Could not convert key {0} for value {1} to type {2}: {3}",
                    key, obj, destType.Name, ex0));
                throw;
            }
        }

        /// <summary>
        /// Returns the current role deployment ID
        /// </summary>
        public static string DeploymentId
        {
            get
            {
                if (RoleEnvironment.IsAvailable)
                {
                    return RoleEnvironment.DeploymentId;
                }
                else
                {
                    return "NON-AZURE";
                }
            }
        }

        /// <summary>
        /// Returns the name of the current role instance or machine
        /// </summary>
        public static string SourceName
        {
            get
            {
                if (RoleEnvironment.IsAvailable)
                {
                    return RoleEnvironment.CurrentRoleInstance.Id;
                }
                else if (IsAzureWebJob())
                {
                    return Environment.GetEnvironmentVariable("WEBJOBS_NAME");
                }
                else
                {
                    return System.Net.Dns.GetHostName();
                }
            }
        }

        public static string RoleName
        {
            get
            {
                if (RoleEnvironment.IsAvailable)
                {
                    return RoleEnvironment.CurrentRoleInstance.Role.Name;
                }
                else
                {
                    return "NOTCLOUD";
                }
            }
        }

        public static int InstanceIndex
        {
            get
            {
                if (RoleEnvironment.IsAvailable)
                {
                    var id = RoleEnvironment.CurrentRoleInstance.Role.Instances.IndexOf(
                        RoleEnvironment.CurrentRoleInstance.Role.Instances.FirstOrDefault(
                            e => e.Id == RoleEnvironment.CurrentRoleInstance.Id));
                    if (id < 0)
                        return 0;
                    else
                        return id;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static string InstanceName
        {
            get
            {
                if (RoleEnvironment.IsAvailable)
                {
                    var id = RoleEnvironment.CurrentRoleInstance.Id;
                    var lastIndex = id.LastIndexOf(".", StringComparison.Ordinal);

                    return (lastIndex > -1)
                        ? id.Substring(lastIndex + 1)
                        : "NOTCLOUD";
                }
                else if (IsAzureWebJob())
                {
                    return Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
                }
                else
                {
                    return "NOTCLOUD";
                }
            }
        }

        /// <summary>
        /// Returns the current role instance local resource name
        /// </summary>
        public static string GetStorageDirectory(string name)
        {
            if (RoleEnvironment.IsAvailable)
            {
                var res = RoleEnvironment.GetLocalResource(name);
                return res.RootPath;
            }
            else
            {
                var path = System.IO.Path.GetTempPath();

                try
                {
                    var fullPath = System.IO.Path.Combine(path, name);
                    if (!System.IO.Directory.Exists(fullPath))
                    {
                        var dirInfo = System.IO.Directory.CreateDirectory(fullPath);
                        return dirInfo.FullName;
                    }
                }
                catch (Exception)
                { }
                return path;
            }
        }

        public static string GetMinutePartition()
        {
            return GetMinutePartition(DateTime.UtcNow);
        }

        public static string GetMinutePartition(DateTime d)
        {
            var now = DateTime.UtcNow;
            var minuteValue = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            var ticks = "0" + minuteValue.Ticks.ToString(CultureInfo.InvariantCulture);
            return ticks;
        }

        public static string GetEventHubName(Uri uri, string eventHubName)
        {
            if (uri == null || string.IsNullOrEmpty(uri.DnsSafeHost))
                return "UNKNOWN";
            if (uri.DnsSafeHost.IndexOf('.') < 0)
                return string.Concat(uri.DnsSafeHost, '/', eventHubName);
            else
                return string.Concat(
                    uri.DnsSafeHost.Substring(0, uri.DnsSafeHost.IndexOf('.')),
                    "/",
                    eventHubName
                );
        }

        public static bool IsAzureWebJob()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
        }
    }
}
