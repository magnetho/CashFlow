using CashFlow.Domain.Enums;

namespace CashFlow.Api.Contracts.CashEntries;

public sealed record CreateCashEntryRequest(
    EntryType Type,
    decimal Amount,
    string Description,
    DateTimeOffset? OccurredAt);
