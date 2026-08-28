using System.Text.Json;
using CashFlow.Application.Abstractions.Time;
using CashFlow.Contracts.Events;
using CashFlow.Domain.Abstractions;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;

namespace CashFlow.Infrastructure.Outbox;

internal sealed class IntegrationEventMapper(
    IAccountingDateResolver accountingDateResolver) : IIntegrationEventMapper
{
    internal const string CashEntryCreatedType = "cash-entry.created.v1";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OutboxMessage? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        CashEntryCreatedDomainEvent createdEvent => Map(createdEvent),
        _ => null
    };

    private OutboxMessage Map(CashEntryCreatedDomainEvent domainEvent)
    {
        var integrationEvent = new CashEntryCreatedIntegrationEvent(
            SchemaVersion: 1,
            domainEvent.EventId,
            domainEvent.EntryId,
            Type: domainEvent.Type == EntryType.Credit ? "credit" : "debit",
            Amount: domainEvent.Amount.Value,
            Description: domainEvent.Description,
            OccurredAtUtc: domainEvent.OccurredAt.ToUniversalTime(),
            AccountingDate: accountingDateResolver.Resolve(domainEvent.OccurredAt));

        return OutboxMessage.Create(
            domainEvent.EventId,
            CashEntryCreatedType,
            JsonSerializer.Serialize(integrationEvent, SerializerOptions),
            domainEvent.OccurredAtUtc);
    }
}
