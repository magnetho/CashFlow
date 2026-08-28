namespace CashFlow.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
