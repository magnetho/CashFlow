using CashFlow.Domain.Exceptions;

namespace CashFlow.Domain.ValueObjects;

public sealed record Money
{
    public const decimal MaximumAmount = 9_999_999_999_999_999.99m;

    private Money()
    {
    }

    private Money(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; private set; }

    public static Money Create(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidMoneyException("O valor deve ser maior que zero.");
        }

        if (amount > MaximumAmount)
        {
            throw new InvalidMoneyException($"O valor não pode exceder {MaximumAmount}.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new InvalidMoneyException("O valor deve ter no máximo duas casas decimais.");
        }

        return new Money(amount);
    }

    public override string ToString() => Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
}
