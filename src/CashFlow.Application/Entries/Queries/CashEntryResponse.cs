using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;

namespace CashFlow.Application.Entries.Queries;

public sealed record CashEntryResponse(
    Guid Id, decimal Amount, EntryType Type, string Description,
    DateTimeOffset OccurredAt, DateTimeOffset CreatedAtUtc)
{
    internal static CashEntryResponse From(CashEntry entry) =>
        new(entry.Id, entry.Amount.Value, entry.Type, entry.Description,
            entry.OccurredAt, entry.CreatedAtUtc);
}
