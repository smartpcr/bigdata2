namespace Telemetry.EventProcessor.DeepStorage.Worker.EventStores
{
	using System;
	public interface IEventStore : IDisposable
	{
		int Level { get; }
		string ReceiveAtHour { get; }
		string PartitionId { get; }
		void Initialize(string partitionId, string receivedAtHour);
		void Write(byte[] value);
		void Flush();
		bool IsFlushOverdue();
	}
}
