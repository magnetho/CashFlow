namespace CashFlow.Contracts.Events;

public sealed record CashEntryCreatedIntegrationEvent(
    int SchemaVersion,
    Guid EventId,
    Guid EntryId,
    string Type,
    decimal Amount,
    string Description,
    DateTimeOffset OccurredAtUtc,
    DateOnly AccountingDate);
