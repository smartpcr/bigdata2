namespace Devices.Simulator
{
    using System;

    public class UpdateTemperatureEvent
    {
        public string DeviceId { get; set; }

        /// <summary>
        /// The observation timestamp (device), UTC offset, stored as ticks 
        /// </summary>
        public DateTime TimeObserved { get; set; }

        /// <summary>
        /// Temperature reading in degrees Centigrade
        /// </summary>
        public float Temperature { get; set; }
    }
}
