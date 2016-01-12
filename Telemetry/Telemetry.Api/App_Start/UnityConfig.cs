using System;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Telemetry.Api.Analytics.EventHubs;
using Telemetry.Api.Analytics.Spec;

namespace Telemetry.Api.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
		private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
		{
			var container = new UnityContainer();
			RegisterDefaults(container);
			RegisterConfig(container);
			return container;
		});

		public static IUnityContainer GetConfiguredContainer()
		{
			return container.Value;
		}

		private static void RegisterConfig(IUnityContainer container)
		{
			if (ConfigurationManager.GetSection("unity") != null)
			{
				container.LoadConfiguration();
			}
		}

		private static void RegisterDefaults(IUnityContainer container)
		{
			container.RegisterType<IEventSender, EventHubEventSender>();
		}
	}
}
