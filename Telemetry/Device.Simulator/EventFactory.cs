namespace Devices.Simulator
{
    using System;

    public static class EventFactory
    {
        public static UpdateTemperatureEvent TemperatureEventFactory(Random random, Device device)
        {
            if (!device.CurrentTemperature.HasValue)
            {
                device.CurrentTemperature = random.Next(25);
            }
            else
            {
                var temperatureChange = random.Next(-2, 3);
                device.CurrentTemperature += temperatureChange;
            }

            return new UpdateTemperatureEvent
            {
                DeviceId = device.Id,
                TimeObserved = DateTime.UtcNow,
                Temperature = device.CurrentTemperature.Value,
            };
        }

        public static UpdateTemperatureEvent ThirtyDegreeTemperatureEventFactory(Random random, Device device)
        {
            return new UpdateTemperatureEvent
            {
                DeviceId = device.Id,
                TimeObserved = DateTime.UtcNow,
                Temperature = 30,
            };
        }
    }
}
