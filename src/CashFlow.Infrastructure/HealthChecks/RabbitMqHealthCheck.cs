using CashFlow.Infrastructure.Messaging.RabbitMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CashFlow.Infrastructure.HealthChecks;

internal sealed class RabbitMqHealthCheck(IOptions<RabbitMqOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var value = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = value.HostName,
                Port = value.Port,
                UserName = value.UserName,
                Password = value.Password,
                VirtualHost = value.VirtualHost
            };
            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy("RabbitMQ disponível.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ indisponível.", exception);
        }
    }
}
