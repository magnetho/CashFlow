using CashFlow.Contracts.Events;
using MongoDB.Driver;

namespace CashFlow.Infrastructure.Persistence.MongoDB;

internal sealed class DailyBalanceProjector(
    MongoDbContext context,
    TimeProvider timeProvider) : IDailyBalanceProjector
{
    public async Task<bool> ProjectAsync(
        CashEntryCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var session = await context.Client.StartSessionAsync(
            cancellationToken: cancellationToken);
        session.StartTransaction();

        try
        {
            await context.InboxMessages.InsertOneAsync(
                session,
                new InboxMessageDocument
                {
                    EventId = integrationEvent.EventId,
                    ProcessedAtUtc = timeProvider.GetUtcNow().UtcDateTime
                },
                cancellationToken: cancellationToken);

            var amount = integrationEvent.Amount;
            var isCredit = integrationEvent.Type.Equals(
                "credit",
                StringComparison.OrdinalIgnoreCase);
            var dateKey = integrationEvent.AccountingDate.ToString("yyyy-MM-dd");

            var update = Builders<DailyBalanceDocument>.Update
                .SetOnInsert(item => item.Date, dateKey)
                .Inc(item => item.TotalCredits, isCredit ? amount : 0m)
                .Inc(item => item.TotalDebits, isCredit ? 0m : amount)
                .Inc(item => item.Balance, isCredit ? amount : -amount)
                .Set(item => item.UpdatedAtUtc, timeProvider.GetUtcNow().UtcDateTime);

            await context.DailyBalances.UpdateOneAsync(
                session,
                item => item.Date == dateKey,
                update,
                new UpdateOptions { IsUpsert = true },
                cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch (MongoWriteException exception)
            when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            await session.AbortTransactionAsync(cancellationToken);
            return false;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }
}
