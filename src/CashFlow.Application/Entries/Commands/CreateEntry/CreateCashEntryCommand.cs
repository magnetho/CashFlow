using CashFlow.Domain.Enums;
using MediatR;

namespace CashFlow.Application.Entries.Commands.CreateEntry;

public sealed record CreateCashEntryCommand(
    decimal Amount,
    EntryType Type,
    string Description,
    DateTimeOffset? OccurredAt) : IRequest<Guid>;
