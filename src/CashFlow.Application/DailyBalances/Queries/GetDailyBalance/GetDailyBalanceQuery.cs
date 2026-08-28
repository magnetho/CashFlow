using CashFlow.Application.Abstractions.Persistence;
using MediatR;

namespace CashFlow.Application.DailyBalances.Queries.GetDailyBalance;

public sealed record GetDailyBalanceQuery(DateOnly Date)
    : IRequest<DailyBalanceReadModel?>;
