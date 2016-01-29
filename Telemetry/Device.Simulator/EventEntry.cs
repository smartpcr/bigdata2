namespace Devices.Simulator
{
    using System;

    public class EventEntry
    {
        private readonly TimeSpan _frequency;
        private readonly double _jitter;
        private readonly Func<Random, Device, object> _eventFactory;
        private readonly Random _random;
        private TimeSpan _frequencyWithJitter;
        private TimeSpan _totalElapsedTime;

        public EventEntry(Func<Random, Device, object> eventFactory, TimeSpan frequency, double jitter)
        {
            _eventFactory = eventFactory;
            _frequency = frequency;
            _jitter = jitter;

            _random=new Random();
            ResetElapsedTime();
        }

        public TimeSpan ElapsedTime
        {
            get { return _totalElapsedTime; }
        }

        public TimeSpan FrequencyWithJitter
        {
            get { return _frequencyWithJitter; }
        }
        public bool ShouldSendEvent()
        {
            return _totalElapsedTime >= _frequencyWithJitter;
        }

        public void UpdateElapsedTime(TimeSpan elapsed)
        {
            _totalElapsedTime += elapsed;
        }

        public object CreateNewEvent(Device device)
        {
            return _eventFactory(_random, device);
        }

        public void ResetElapsedTime()
        {
            // Figure out how much time is left over after the last event was triggered.
            var remainder = _totalElapsedTime - _frequencyWithJitter;

            // Start measuring the elapsed time from either zero, or the remainder of the last time around.
            _totalElapsedTime = remainder < TimeSpan.Zero ? TimeSpan.Zero : remainder;

            // Compute the next random jitter factor, centered on zero.
            // For example, if _jitter == 0.1, then we'd like a random value from -0.1 to +0.1.
            var nextJitter = (_random.NextDouble() * 2 * _jitter) - _jitter;

            // Apply the jitter.
            _frequencyWithJitter = _frequency + _frequency.Multiply(nextJitter);
        }
    }

    public static class Extensions
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
        {
            return TimeSpan.FromTicks((long)(timeSpan.Ticks * factor));
        }
    }
}
