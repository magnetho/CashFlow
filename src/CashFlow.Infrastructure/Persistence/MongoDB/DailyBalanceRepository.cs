using CashFlow.Application.Abstractions.Persistence;
using MongoDB.Driver;

namespace CashFlow.Infrastructure.Persistence.MongoDB;

internal sealed class DailyBalanceRepository(MongoDbContext context)
    : IDailyBalanceReadRepository
{
    public async Task<DailyBalanceReadModel?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dateKey = date.ToString("yyyy-MM-dd");
        var document = await context.DailyBalances
            .Find(item => item.Date == dateKey)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null
            ? null
            : new DailyBalanceReadModel(
                date,
                document.TotalCredits,
                document.TotalDebits,
                document.Balance,
                new DateTimeOffset(document.UpdatedAtUtc, TimeSpan.Zero));
    }
}
