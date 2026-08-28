using System.Text.Json;
using CashFlow.Application.Time;
using CashFlow.Contracts.Events;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;
using CashFlow.Domain.ValueObjects;
using CashFlow.Infrastructure.Outbox;

namespace CashFlow.IntegrationTests.Outbox;

public sealed class IntegrationEventMapperTests
{
    [Fact]
    public void Map_WhenCashEntryWasCreated_CreatesVersionedOutboxMessage()
    {
        var occurredAt = new DateTimeOffset(2026, 8, 27, 23, 30, 0, TimeSpan.FromHours(-3));
        var createdAtUtc = occurredAt.AddMinutes(1).ToUniversalTime();
        var entry = CashEntry.Create(
            Money.Create(150.50m), EntryType.Credit, "Product sale", occurredAt, createdAtUtc);
        var domainEvent = Assert.IsType<CashEntryCreatedDomainEvent>(Assert.Single(entry.DomainEvents));
        var mapper = new IntegrationEventMapper(new AccountingDateResolver("America/Sao_Paulo"));

        var outboxMessage = mapper.Map(domainEvent);

        Assert.NotNull(outboxMessage);
        Assert.Equal(domainEvent.EventId, outboxMessage.Id);
        Assert.Equal(IntegrationEventMapper.CashEntryCreatedType, outboxMessage.Type);
        Assert.Null(outboxMessage.ProcessedAtUtc);
        Assert.Equal(0, outboxMessage.RetryCount);

        var integrationEvent = JsonSerializer.Deserialize<CashEntryCreatedIntegrationEvent>(
            outboxMessage.Payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(integrationEvent);
        Assert.Equal(1, integrationEvent.SchemaVersion);
        Assert.Equal(entry.Id, integrationEvent.EntryId);
        Assert.Equal("credit", integrationEvent.Type);
        Assert.Equal(150.50m, integrationEvent.Amount);
        Assert.Equal("Product sale", integrationEvent.Description);
        Assert.Equal(new DateOnly(2026, 8, 27), integrationEvent.AccountingDate);
        Assert.Equal(occurredAt.ToUniversalTime(), integrationEvent.OccurredAtUtc);
    }
}
