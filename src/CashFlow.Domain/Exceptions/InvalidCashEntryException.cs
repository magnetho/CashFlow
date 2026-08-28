namespace CashFlow.Domain.Exceptions;

public sealed class InvalidCashEntryException(string message) : DomainException(message);
