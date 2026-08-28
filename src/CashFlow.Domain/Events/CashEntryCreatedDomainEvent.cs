using CashFlow.Domain.Abstractions;
using CashFlow.Domain.Enums;
using CashFlow.Domain.ValueObjects;

namespace CashFlow.Domain.Events;

public sealed record CashEntryCreatedDomainEvent(
    Guid EventId,
    Guid EntryId,
    Money Amount,
    EntryType Type,
    string Description,
    DateTimeOffset OccurredAt,
    DateTimeOffset OccurredAtUtc) : IDomainEvent;
