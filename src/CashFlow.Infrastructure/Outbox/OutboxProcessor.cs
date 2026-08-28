using CashFlow.Infrastructure.Messaging.RabbitMQ;
using CashFlow.Infrastructure.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CashFlow.Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    CashFlowDbContext dbContext,
    IIntegrationEventPublisher publisher,
    TimeProvider timeProvider,
    IOptions<OutboxProcessorOptions> options,
    ILogger<OutboxProcessor> logger) : IOutboxProcessor
{
    private readonly OutboxProcessorOptions _options = options.Value;

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var nowUtc = timeProvider.GetUtcNow();

        var messages = await dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM outbox_messages
                WHERE processed_at_utc IS NULL
                  AND (next_attempt_at_utc IS NULL OR next_attempt_at_utc <= {nowUtc})
                ORDER BY occurred_at_utc
                FOR UPDATE SKIP LOCKED
                LIMIT {_options.BatchSize}
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(message, cancellationToken);
                message.MarkAsProcessed(timeProvider.GetUtcNow());

                logger.LogInformation(
                    "OutboxMessagePublished: {EventId} {EventType}",
                    message.Id,
                    message.Type);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                message.MarkAsFailed(timeProvider.GetUtcNow(), exception.Message);

                logger.LogWarning(
                    exception,
                    "OutboxMessagePublishFailed: {EventId} attempt {RetryCount}",
                    message.Id,
                    message.RetryCount);

                break;
            }
        }

        if (messages.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return messages.Count;
    }
}
