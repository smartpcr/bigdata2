namespace Telemetry.Core
{
	using System.Configuration;
	using Microsoft.Practices.Unity;
	using Microsoft.Practices.Unity.Configuration;

	public static class Container
	{
		public static IUnityContainer Instance { get; private set; }

		static Container()
		{
			Instance = new UnityContainer();
			RegisterMandatoryComponents();
			RegisterOptionalComponents();
			RegisterConfiguredComponents();
		}

		private static void RegisterMandatoryComponents()
		{
		}

		private static void RegisterConfiguredComponents()
		{
			if (ConfigurationManager.GetSection("unity") != null)
			{
				Instance.LoadConfiguration();
			}
		}

		private static void RegisterOptionalComponents()
		{
		}
	}
}
