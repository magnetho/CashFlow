using CashFlow.Application.Abstractions.Persistence;
using CashFlow.Infrastructure.Persistence.PostgreSQL;
using CashFlow.Infrastructure.Persistence.PostgreSQL.Repositories;
using CashFlow.Infrastructure.Outbox;
using CashFlow.Infrastructure.Messaging.RabbitMQ;
using CashFlow.Infrastructure.Persistence.MongoDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CashFlow.Infrastructure.HealthChecks;

namespace CashFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException(
                "Connection string 'PostgreSql' was not configured.");

        services.AddDbContext<CashFlowDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICashEntryRepository, CashEntryRepository>();
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<CashFlowDbContext>());
        services.AddSingleton<IIntegrationEventMapper, IntegrationEventMapper>();

        services.AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "MongoDB connection string must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName),
                "MongoDB database name must be configured.")
            .ValidateOnStart();
        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IDailyBalanceReadRepository, DailyBalanceRepository>();
        services.AddScoped<IDailyBalanceProjector, DailyBalanceProjector>();
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName));

        return services;
    }

    public static IServiceCollection AddOutboxPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.HostName),
                "RabbitMQ hostname must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Exchange),
                "RabbitMQ exchange must be configured.")
            .ValidateOnStart();

        services.AddOptions<OutboxProcessorOptions>()
            .Bind(configuration.GetSection(OutboxProcessorOptions.SectionName))
            .Validate(options => options.BatchSize is > 0 and <= 500,
                "Outbox batch size must be between 1 and 500.")
            .Validate(options => options.PollingIntervalMilliseconds is >= 100 and <= 60_000,
                "Outbox polling interval must be between 100 and 60000 milliseconds.")
            .ValidateOnStart();

        services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }

    public static IServiceCollection AddIntegrationEventConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IIntegrationEventConsumer, RabbitMqIntegrationEventConsumer>();

        return services;
    }

    public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgres = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not configured.");
        services.AddHealthChecks()
            .AddCheck("postgresql", new PostgreSqlHealthCheck(postgres))
            .AddCheck<MongoDbHealthCheck>("mongodb")
            .AddCheck<RabbitMqHealthCheck>("rabbitmq");
        return services;
    }
}
