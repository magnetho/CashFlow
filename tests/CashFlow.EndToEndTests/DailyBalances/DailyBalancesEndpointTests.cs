using System.Net;
using System.Net.Http.Json;
using CashFlow.EndToEndTests.CashEntries;

namespace CashFlow.EndToEndTests.DailyBalances;

public sealed class DailyBalancesEndpointTests(CashFlowApiFactory factory)
    : IClassFixture<CashFlowApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_WhenBalanceExists_ReturnsDailyConsolidation()
    {
        var response = await _client.GetAsync("/api/v1/daily-balances/2026-08-27");
        var body = await response.Content.ReadFromJsonAsync<DailyBalanceResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(new DateOnly(2026, 8, 27), body.Date);
        Assert.Equal(300m, body.TotalCredits);
        Assert.Equal(50m, body.TotalDebits);
        Assert.Equal(250m, body.Balance);
    }

    [Fact]
    public async Task Get_WhenBalanceDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/daily-balances/2026-08-28");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record DailyBalanceResponse(
        DateOnly Date,
        decimal TotalCredits,
        decimal TotalDebits,
        decimal Balance,
        DateTimeOffset UpdatedAtUtc);
}
