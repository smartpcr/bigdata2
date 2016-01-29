using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices.Simulator.Console
{
    using System.IO;
    using System.Threading;
    using Devices.Core;
    using Devices.Core.Logging;
    using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;

    class Program
    {
        private static FileSystemWatcher _fileSystemWatcher;
        private static SimulationProfile _deviceSimulator;

        static void Main(string[] args)
        {
            var observableEventListener = new ObservableEventListener();
            var configuration = SimulatorConfiguration.GetCurrentConfiguration();
            observableEventListener.EnableEvents(ScenarioSimulatorEventSource.Log, configuration.GetLogLevel());
            observableEventListener.LogToConsole();

            var hostName = ConfigurationHelper.SourceName;
            _deviceSimulator=new SimulationProfile(hostName, configuration);
            if (args.Length > 0)
            {
                var scenario = args.Contains("/default", StringComparer.OrdinalIgnoreCase)
                    ? SimulationScenarios.DefaultScenario()
                    : args.First(x => !x.StartsWith("/", StringComparison.Ordinal));

                var ct = args.Contains("/webjob", StringComparer.OrdinalIgnoreCase)
                    ? GetWebJobCancellationToken()
                    : CancellationToken.None;

                ProvisionDevicesAsync(ct).Wait(ct);
                _deviceSimulator.RunSimulationAsync(scenario, ct).Wait(ct);

                return;
            }


            var options = new Dictionary<string, Func<CancellationToken, Task>>();
            options.Add("Provision Devices", ProvisionDevicesAsync);
            // no command line arguments, so prompt with a menu.
            foreach (var scenario in SimulationScenarios.AllScenarios)
            {
                options.Add("Run " + scenario, (Func<CancellationToken, Task>)(token => _deviceSimulator.RunSimulationAsync(scenario, token)));
            }
            //options.Add("Deprovision Devices", DeprovisionDevicesAsync);
            ConsoleHost.RunWithOptionsAsync(options).Wait();
        }

        private static async Task ProvisionDevicesAsync(CancellationToken token)
        {
            _deviceSimulator.ProvisionDevices(true);
            await Task.Delay(0);
        }

        private static CancellationToken GetWebJobCancellationToken()
        {
            var shutdownFile = Environment.GetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE");
            var directory = Path.GetDirectoryName(shutdownFile);
            if (directory == null)
                return CancellationToken.None;

            var cts = new CancellationTokenSource();
            _fileSystemWatcher = new FileSystemWatcher(directory);
            _fileSystemWatcher.Created += (sender, args) =>
            {
                if (args.FullPath.Equals(Path.GetFullPath(shutdownFile), StringComparison.OrdinalIgnoreCase))
                    cts.Cancel();
            };

            _fileSystemWatcher.EnableRaisingEvents = true;
            return cts.Token;
        }
    }
}
