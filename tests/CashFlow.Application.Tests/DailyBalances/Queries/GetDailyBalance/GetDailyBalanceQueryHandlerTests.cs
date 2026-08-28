using CashFlow.Application.Abstractions.Persistence;
using CashFlow.Application.DailyBalances.Queries.GetDailyBalance;

namespace CashFlow.Application.Tests.DailyBalances.Queries.GetDailyBalance;

public sealed class GetDailyBalanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenBalanceExists_ReturnsReadModel()
    {
        var date = new DateOnly(2026, 8, 27);
        var expected = new DailyBalanceReadModel(
            date, 300m, 50m, 250m, DateTimeOffset.UtcNow);
        var handler = new GetDailyBalanceQueryHandler(
            new StubDailyBalanceRepository(expected));

        var result = await handler.Handle(
            new GetDailyBalanceQuery(date),
            CancellationToken.None);

        Assert.Equal(expected, result);
    }

    private sealed class StubDailyBalanceRepository(DailyBalanceReadModel result)
        : IDailyBalanceReadRepository
    {
        public Task<DailyBalanceReadModel?> GetByDateAsync(
            DateOnly date,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<DailyBalanceReadModel?>(result);
    }
}
