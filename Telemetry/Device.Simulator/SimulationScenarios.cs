using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices.Simulator
{
    using System.Reflection;
    using Devices.Core.Logging;
    using EventGenerator = Func<global::Devices.Simulator.EventEntry[]>;

    public static class SimulationScenarios
    {
        private static readonly Dictionary<string, EventGenerator> ScenarioMap;

        static SimulationScenarios()
        {
            ScenarioMap =
                typeof(SimulationScenarios).GetTypeInfo()
                    .DeclaredMethods
                    .Where(x => x.ReturnType == typeof(EventEntry[]))
                    .ToDictionary(
                        x => x.Name,
                        x => (EventGenerator)x.CreateDelegate(typeof(EventGenerator)));
        }

        public static EventEntry[] NoErrorsExpected()
        {
            return new[]
                       {
                           new EventEntry(EventFactory.TemperatureEventFactory, TimeSpan.FromSeconds(1), 0.1)
                       };
        }

        public static EventEntry[] ThirtyDegreeReadings()
        {
            return new[]
                       {
                           new EventEntry(EventFactory.ThirtyDegreeTemperatureEventFactory, TimeSpan.FromSeconds(10), 0.1)
                       };
        }

        public static IReadOnlyList<string> AllScenarios
        {
            get { return ScenarioMap.Keys.ToList(); }
        }

        public static EventGenerator GetScenarioByName(string scenario)
        {
            EventGenerator generator;
            if (!ScenarioMap.TryGetValue(scenario, out generator))
            {
                var ex = new KeyNotFoundException("The specified scenario, " + scenario + ", was not recognized.");
                ScenarioSimulatorEventSource.Log.UnknownScenario(scenario, ex);
                throw ex;
            }

            return generator;
        }

        public static string DefaultScenario()
        {
            EventGenerator func = NoErrorsExpected;
            return func.Method.Name;
        }
    }
}
