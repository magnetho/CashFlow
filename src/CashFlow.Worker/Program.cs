using CashFlow.Worker;
using CashFlow.Application;
using CashFlow.Infrastructure;
using CashFlow.Infrastructure.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var accountingTimeZoneId = builder.Configuration["CashFlow:AccountingTimeZone"]
    ?? "America/Sao_Paulo";

builder.Services.AddApplication(accountingTimeZoneId);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutboxPublisher(builder.Configuration);
builder.Services.AddIntegrationEventConsumer(builder.Configuration);
builder.Services.AddHostedService<OutboxPublisherWorker>();
builder.Services.AddHostedService<DailyBalanceConsumerWorker>();

var host = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = host.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();
    await dbContext.Database.MigrateAsync();
}

await host.RunAsync();
