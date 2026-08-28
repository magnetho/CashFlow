using CashFlow.Contracts.Events;
using CashFlow.Infrastructure.Persistence.MongoDB;
using Microsoft.Extensions.Options;
using Testcontainers.MongoDb;

namespace CashFlow.IntegrationTests.Persistence;

public sealed class DailyBalanceProjectorTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:8.0")
        .WithReplicaSet("rs0")
        .Build();

    public Task InitializeAsync() => _mongo.StartAsync();

    public Task DisposeAsync() => _mongo.DisposeAsync().AsTask();

    [Fact]
    public async Task ProjectAsync_WhenEventIsDeliveredTwice_UpdatesBalanceOnlyOnce()
    {
        var options = Options.Create(new MongoDbOptions
        {
            ConnectionString = _mongo.GetConnectionString(),
            DatabaseName = $"cashflow_tests_{Guid.NewGuid():N}"
        });
        var context = new MongoDbContext(options);
        var now = new DateTimeOffset(2026, 8, 27, 15, 0, 0, TimeSpan.Zero);
        var projector = new DailyBalanceProjector(context, new FixedTimeProvider(now));
        var repository = new DailyBalanceRepository(context);
        var integrationEvent = new CashEntryCreatedIntegrationEvent(
            1, Guid.NewGuid(), Guid.NewGuid(), "credit", 100m, "Venda", now,
            new DateOnly(2026, 8, 27));

        var firstProcessing = await projector.ProjectAsync(integrationEvent);
        var duplicateProcessing = await projector.ProjectAsync(integrationEvent);
        var balance = await repository.GetByDateAsync(integrationEvent.AccountingDate);

        Assert.True(firstProcessing);
        Assert.False(duplicateProcessing);
        Assert.NotNull(balance);
        Assert.Equal(100m, balance.TotalCredits);
        Assert.Equal(0m, balance.TotalDebits);
        Assert.Equal(100m, balance.Balance);
    }

    [Fact]
    public async Task ProjectAsync_WhenCreditAndDebitAreProcessed_CalculatesBalance()
    {
        var options = Options.Create(new MongoDbOptions
        {
            ConnectionString = _mongo.GetConnectionString(),
            DatabaseName = $"cashflow_tests_{Guid.NewGuid():N}"
        });
        var context = new MongoDbContext(options);
        var now = new DateTimeOffset(2026, 8, 27, 15, 0, 0, TimeSpan.Zero);
        var projector = new DailyBalanceProjector(context, new FixedTimeProvider(now));
        var repository = new DailyBalanceRepository(context);
        var date = new DateOnly(2026, 8, 27);

        await projector.ProjectAsync(CreateEvent("credit", 300m, date, now));
        await projector.ProjectAsync(CreateEvent("debit", 50m, date, now));
        var balance = await repository.GetByDateAsync(date);

        Assert.NotNull(balance);
        Assert.Equal(300m, balance.TotalCredits);
        Assert.Equal(50m, balance.TotalDebits);
        Assert.Equal(250m, balance.Balance);
    }

    private static CashEntryCreatedIntegrationEvent CreateEvent(
        string type,
        decimal amount,
        DateOnly date,
        DateTimeOffset occurredAt) =>
        new(1, Guid.NewGuid(), Guid.NewGuid(), type, amount, "Teste", occurredAt, date);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
