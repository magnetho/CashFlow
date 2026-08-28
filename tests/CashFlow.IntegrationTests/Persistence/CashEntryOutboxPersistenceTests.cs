using CashFlow.Application.Time;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.ValueObjects;
using CashFlow.Infrastructure.Outbox;
using CashFlow.Infrastructure.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CashFlow.IntegrationTests.Persistence;

public sealed class CashEntryOutboxPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("cashflow_tests")
        .WithUsername("cashflow")
        .WithPassword("cashflow_tests")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task SaveChanges_WhenCashEntryIsCreated_PersistsEntryAndOutboxTogether()
    {
        await using var dbContext = CreateDbContext();
        var occurredAt = new DateTimeOffset(2026, 8, 27, 14, 30, 0, TimeSpan.FromHours(-3));
        var cashEntry = CashEntry.Create(
            Money.Create(150.50m),
            EntryType.Credit,
            "Venda de produto",
            occurredAt,
            occurredAt.AddMinutes(1));

        await dbContext.CashEntries.AddAsync(cashEntry);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();
        var persistedEntry = await dbContext.CashEntries.SingleAsync();
        var outboxMessage = await dbContext.OutboxMessages.SingleAsync();

        Assert.Equal(cashEntry.Id, persistedEntry.Id);
        Assert.Equal(occurredAt.ToUniversalTime(), persistedEntry.OccurredAt);
        Assert.Equal("Venda de produto", persistedEntry.Description);
        Assert.Equal(IntegrationEventMapper.CashEntryCreatedType, outboxMessage.Type);
        Assert.Contains("Venda de produto", outboxMessage.Payload);
        Assert.Null(outboxMessage.ProcessedAtUtc);
        Assert.Empty(cashEntry.DomainEvents);
    }

    private CashFlowDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CashFlowDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        var mapper = new IntegrationEventMapper(
            new AccountingDateResolver("America/Sao_Paulo"));

        return new CashFlowDbContext(options, mapper);
    }
}
