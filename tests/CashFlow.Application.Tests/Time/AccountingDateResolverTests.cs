using CashFlow.Application.Time;

namespace CashFlow.Application.Tests.Time;

public sealed class AccountingDateResolverTests
{
    private readonly AccountingDateResolver _resolver = new("America/Sao_Paulo");

    [Fact]
    public void Resolve_WhenUtcInstantIsBeforeMidnightInBrasilia_ReturnsPreviousDate()
    {
        var instant = new DateTimeOffset(2026, 8, 28, 1, 30, 0, TimeSpan.Zero);

        var accountingDate = _resolver.Resolve(instant);

        Assert.Equal(new DateOnly(2026, 8, 27), accountingDate);
    }

    [Fact]
    public void Resolve_WhenUtcInstantIsAfterMidnightInBrasilia_ReturnsCurrentDate()
    {
        var instant = new DateTimeOffset(2026, 8, 28, 3, 30, 0, TimeSpan.Zero);

        var accountingDate = _resolver.Resolve(instant);

        Assert.Equal(new DateOnly(2026, 8, 28), accountingDate);
    }

    [Fact]
    public void Resolve_ForHistoricalDate_UsesHistoricalDaylightSavingRule()
    {
        var instant = new DateTimeOffset(2018, 11, 4, 2, 30, 0, TimeSpan.Zero);

        var accountingDate = _resolver.Resolve(instant);

        Assert.Equal(new DateOnly(2018, 11, 3), accountingDate);
    }
}
