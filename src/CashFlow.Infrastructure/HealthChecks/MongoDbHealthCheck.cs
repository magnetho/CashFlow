using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using CashFlow.Infrastructure.Persistence.MongoDB;

namespace CashFlow.Infrastructure.HealthChecks;

internal sealed class MongoDbHealthCheck(IOptions<MongoDbOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new MongoClient(options.Value.ConnectionString);
            await client.GetDatabase(options.Value.DatabaseName)
                .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB disponível.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB indisponível.", exception);
        }
    }
}
