namespace Telemetry.EventProcessor.DeepStorage.Worker
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.ServiceBus.Messaging;
	using Telemetry.Core;
	using Telemetry.EventProcessor.DeepStorage.Worker.EventProcessors;

	public class EventReceiver
	{
		private readonly EventProcessorHost _host;

		public EventReceiver()
		{
			_host = new EventProcessorHost(
				Environment.MachineName,
				Config.Get("DeepStorage.EventHubName"),
				Config.Get("DeepStorage.ConsumerGroupName"),
				Config.Get("DeepStorage.InputConnectionString"),
				Config.Get("DeepStorage.CheckpointConnectionString"));
		}

		public async Task RegisterProcessorAsync()
		{
			var processorOptions = new EventProcessorOptions()
			{
				MaxBatchSize = 5000,
				PrefetchCount = 1000
			};
			await _host.RegisterEventProcessorAsync<DeepStorageEventProcessor>(processorOptions);
		}

		public async Task UnregisterProcessorAsync()
		{
			if (_host != null)
			{
				await _host.UnregisterEventProcessorAsync();
			}
		}
	}
}
