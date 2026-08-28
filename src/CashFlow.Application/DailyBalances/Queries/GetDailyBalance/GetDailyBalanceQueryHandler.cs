using CashFlow.Application.Abstractions.Persistence;
using MediatR;

namespace CashFlow.Application.DailyBalances.Queries.GetDailyBalance;

internal sealed class GetDailyBalanceQueryHandler(
    IDailyBalanceReadRepository repository)
    : IRequestHandler<GetDailyBalanceQuery, DailyBalanceReadModel?>
{
    public Task<DailyBalanceReadModel?> Handle(
        GetDailyBalanceQuery request,
        CancellationToken cancellationToken) =>
        repository.GetByDateAsync(request.Date, cancellationToken);
}
