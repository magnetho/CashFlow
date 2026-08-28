using CashFlow.Domain.Abstractions;

namespace CashFlow.Infrastructure.Outbox;

public interface IIntegrationEventMapper
{
    OutboxMessage? Map(IDomainEvent domainEvent);
}
