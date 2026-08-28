namespace CashFlow.Infrastructure.Outbox;

public sealed class OutboxProcessorOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; init; } = 100;

    public int PollingIntervalMilliseconds { get; init; } = 1_000;
}
