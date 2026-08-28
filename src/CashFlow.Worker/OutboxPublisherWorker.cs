using CashFlow.Infrastructure.Outbox;
using Microsoft.Extensions.Options;

namespace CashFlow.Worker;

public sealed class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxProcessorOptions> options,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    private readonly TimeSpan _pollingInterval =
        TimeSpan.FromMilliseconds(options.Value.PollingIntervalMilliseconds);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxPublisherStarted");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                var processedCount = await processor.ProcessBatchAsync(stoppingToken);

                if (processedCount == 0)
                {
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "OutboxPublisherIterationFailed");
                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }

        logger.LogInformation("OutboxPublisherStopped");
    }
}
