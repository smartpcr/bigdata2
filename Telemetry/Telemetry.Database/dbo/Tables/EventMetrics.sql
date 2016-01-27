CREATE TABLE [dbo].[EventMetrics]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[EventName] NVARCHAR(40) NOT NULL,
	[Period] NVARCHAR(10) NOT NULL,
	[PartitionId] NVARCHAR(5) NOT NULL,
	[Count] BIGINT NULL,
	[ProcessedAt] DATETIME NULL
)
GO

CREATE INDEX [IX_EventMetrics_Period_EventName_PartitionId] 
ON [dbo].[EventMetrics] (Period, EventName, PartitionId)
