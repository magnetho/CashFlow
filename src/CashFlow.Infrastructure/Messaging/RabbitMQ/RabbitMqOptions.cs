namespace CashFlow.Infrastructure.Messaging.RabbitMQ;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";

    public string Exchange { get; init; } = "cashflow.events";

    public string Queue { get; init; } = "cashflow.daily-balance.v1";

    public string RoutingKey { get; init; } = "cash-entry.created.v1";

    public string DeadLetterExchange { get; init; } = "cashflow.dead-letter";

    public string DeadLetterQueue { get; init; } = "cashflow.daily-balance.v1.dead-letter";
}
