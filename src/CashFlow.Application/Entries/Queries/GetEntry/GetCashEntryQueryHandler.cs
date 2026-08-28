using CashFlow.Application.Abstractions.Persistence;
using MediatR;

namespace CashFlow.Application.Entries.Queries.GetEntry;

internal sealed class GetCashEntryQueryHandler(ICashEntryRepository repository)
    : IRequestHandler<GetCashEntryQuery, CashEntryResponse?>
{
    public async Task<CashEntryResponse?> Handle(GetCashEntryQuery request, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(request.Id, cancellationToken);
        return entry is null ? null : CashEntryResponse.From(entry);
    }
}
