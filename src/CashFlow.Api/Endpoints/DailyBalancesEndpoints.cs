using CashFlow.Application.DailyBalances.Queries.GetDailyBalance;
using MediatR;

namespace CashFlow.Api.Endpoints;

public static class DailyBalancesEndpoints
{
    public static IEndpointRouteBuilder MapDailyBalancesEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/daily-balances")
            .WithTags("Daily Balances");

        group.MapGet("/{date}", GetDailyBalance)
            .WithName("GetDailyBalance")
            .Produces<DailyBalanceResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> GetDailyBalance(
        DateOnly date,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var balance = await sender.Send(
            new GetDailyBalanceQuery(date),
            cancellationToken);

        return balance is null
            ? Results.NotFound()
            : Results.Ok(new DailyBalanceResponse(
                balance.Date,
                balance.TotalCredits,
                balance.TotalDebits,
                balance.Balance,
                balance.UpdatedAtUtc));
    }

    private sealed record DailyBalanceResponse(
        DateOnly Date,
        decimal TotalCredits,
        decimal TotalDebits,
        decimal Balance,
        DateTimeOffset UpdatedAtUtc);
}
