namespace CashFlow.Application.Abstractions.Persistence;

public interface IDailyBalanceReadRepository
{
    Task<DailyBalanceReadModel?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);
}

public sealed record DailyBalanceReadModel(
    DateOnly Date,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    DateTimeOffset UpdatedAtUtc);
