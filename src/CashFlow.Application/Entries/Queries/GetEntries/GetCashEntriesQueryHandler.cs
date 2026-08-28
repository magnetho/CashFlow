using CashFlow.Application.Abstractions.Persistence;
using MediatR;

namespace CashFlow.Application.Entries.Queries.GetEntries;

internal sealed class GetCashEntriesQueryHandler(ICashEntryRepository repository)
    : IRequestHandler<GetCashEntriesQuery, PagedCashEntriesResponse>
{
    public async Task<PagedCashEntriesResponse> Handle(GetCashEntriesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPageAsync(
            request.Page, request.PageSize, request.Type, cancellationToken);
        return new PagedCashEntriesResponse(
            items.Select(CashEntryResponse.From).ToArray(), request.Page, request.PageSize,
            totalCount, (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}
