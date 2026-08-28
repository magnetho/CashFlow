using RabbitMQ.Client;

namespace CashFlow.Infrastructure.Messaging.RabbitMQ;

internal interface IRabbitMqTopology
{
    Task DeclareAsync(IChannel channel, CancellationToken cancellationToken = default);
}
