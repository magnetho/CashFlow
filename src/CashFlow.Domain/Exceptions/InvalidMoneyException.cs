namespace CashFlow.Domain.Exceptions;

public sealed class InvalidMoneyException(string message) : DomainException(message);
