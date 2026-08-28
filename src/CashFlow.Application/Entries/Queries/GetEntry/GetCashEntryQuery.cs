using CashFlow.Application.Entries.Queries;
using MediatR;

namespace CashFlow.Application.Entries.Queries.GetEntry;

public sealed record GetCashEntryQuery(Guid Id) : IRequest<CashEntryResponse?>;
