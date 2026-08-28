using CashFlow.Application.Entries.Queries;
using CashFlow.Domain.Enums;
using MediatR;

namespace CashFlow.Application.Entries.Queries.GetEntries;

public sealed record GetCashEntriesQuery(int Page, int PageSize, EntryType? Type)
    : IRequest<PagedCashEntriesResponse>;

public sealed record PagedCashEntriesResponse(
    IReadOnlyList<CashEntryResponse> Items, int Page, int PageSize,
    int TotalCount, int TotalPages);
