﻿namespace Devices.Simulator
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Globalization;

    public class SimulatorConfiguration
    {
        public string Scenario { get; set; }

        public int NumberOfDevices { get; set; }

        public string EventHubNamespace { get; set; }

        public string EventHubSasKeyName { get; set; }

        public string EventHubPrimaryKey { get; set; }

        public int EventHubTokenLifetimeDays { get; set; }

        public string EventHubName { get; set; }

        public TimeSpan WarmUpDuration { get; set; }

        private EventLevel _eventLevel;
        public EventLevel GetLogLevel()
        {
            const string Key = "IOT_LOGLEVEL";
            object logLevel = Environment.GetEnvironmentVariable(Key);
            if (logLevel != null)
            {
                _eventLevel = ConfigurationHelper.ConvertValue<EventLevel>(Key, logLevel);
            }

            return _eventLevel;
        }

        public static SimulatorConfiguration GetCurrentConfiguration()
        {
            return new SimulatorConfiguration
            {
                Scenario = ConfigurationHelper.GetConfigValue<string>("Simulator.Scenario", String.Empty),
                NumberOfDevices = ConfigurationHelper.GetConfigValue<int>("Simulator.NumberOfDevices"),
                EventHubNamespace = ConfigurationHelper.GetConfigValue<string>("Simulator.EventHubNamespace"),
                EventHubName = ConfigurationHelper.GetConfigValue<string>("Simulator.EventHubName"),
                EventHubSasKeyName = ConfigurationHelper.GetConfigValue<string>("Simulator.EventHubSasKeyName"),
                EventHubPrimaryKey = ConfigurationHelper.GetConfigValue<string>("Simulator.EventHubPrimaryKey"),
                EventHubTokenLifetimeDays = ConfigurationHelper.GetConfigValue<int>("Simulator.EventHubTokenLifetimeDays", 7),
                WarmUpDuration = ConfigurationHelper.GetConfigValue("Simulator.WarmUpDuration", TimeSpan.FromSeconds(30)),
                _eventLevel = ConfigurationHelper.GetConfigValue<EventLevel>("Simulator.LogLevel", EventLevel.Informational)
            };
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "Simulation SimulatorConfiguration; device count = {0} event hub name = {1}",
                NumberOfDevices,
                EventHubName);
        }
    }
}
