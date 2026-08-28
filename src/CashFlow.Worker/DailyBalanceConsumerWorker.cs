using CashFlow.Infrastructure.Messaging.RabbitMQ;

namespace CashFlow.Worker;

internal sealed class DailyBalanceConsumerWorker(
    IIntegrationEventConsumer consumer,
    ILogger<DailyBalanceConsumerWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await consumer.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Consumidor indisponível; nova tentativa em 5 segundos");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
