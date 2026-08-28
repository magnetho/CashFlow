namespace CashFlow.Application.Abstractions.Time;

public interface IAccountingDateResolver
{
    DateOnly Resolve(DateTimeOffset instant);
}
