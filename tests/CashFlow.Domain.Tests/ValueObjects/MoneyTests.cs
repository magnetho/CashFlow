using CashFlow.Domain.Exceptions;
using CashFlow.Domain.ValueObjects;

namespace CashFlow.Domain.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Theory]
    [InlineData("0.01")]
    [InlineData("100")]
    [InlineData("150.50")]
    public void Create_WhenAmountIsValid_ReturnsMoney(string value)
    {
        var amount = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        var money = Money.Create(amount);

        Assert.Equal(amount, money.Value);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("-100")]
    public void Create_WhenAmountIsNotPositive_ThrowsInvalidMoneyException(string value)
    {
        var amount = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        var exception = Assert.Throws<InvalidMoneyException>(() => Money.Create(amount));

        Assert.Equal("O valor deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Create_WhenAmountHasMoreThanTwoDecimalPlaces_ThrowsInvalidMoneyException()
    {
        var exception = Assert.Throws<InvalidMoneyException>(() => Money.Create(10.001m));

        Assert.Equal("O valor deve ter no máximo duas casas decimais.", exception.Message);
    }

    [Fact]
    public void Create_WhenAmountExceedsDatabasePrecision_ThrowsInvalidMoneyException()
    {
        var exception = Assert.Throws<InvalidMoneyException>(() => Money.Create(Money.MaximumAmount + 0.01m));

        Assert.Contains("não pode exceder", exception.Message);
    }

    [Fact]
    public void Money_WithSameValue_HasValueEquality()
    {
        var first = Money.Create(100.50m);
        var second = Money.Create(100.50m);

        Assert.Equal(first, second);
    }
}
