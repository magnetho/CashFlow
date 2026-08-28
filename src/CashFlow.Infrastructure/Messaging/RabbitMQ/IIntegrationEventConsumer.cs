namespace CashFlow.Infrastructure.Messaging.RabbitMQ;

public interface IIntegrationEventConsumer
{
    Task RunAsync(CancellationToken cancellationToken);
}
