using CashFlow.Infrastructure.Outbox;

namespace CashFlow.Infrastructure.Messaging.RabbitMQ;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
