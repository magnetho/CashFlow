using CashFlow.Contracts.Events;

namespace CashFlow.Infrastructure.Persistence.MongoDB;

public interface IDailyBalanceProjector
{
    Task<bool> ProjectAsync(
        CashEntryCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
