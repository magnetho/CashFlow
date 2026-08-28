using CashFlow.Application.Time;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;
using CashFlow.Domain.ValueObjects;
using CashFlow.Infrastructure.Outbox;

namespace CashFlow.IntegrationTests.Outbox;

public sealed class OutboxMessageTests
{
    [Fact]
    public void MarkAsProcessed_ClearsFailureState()
    {
        var message = CreateMessage();
        var failedAt = new DateTimeOffset(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);
        message.MarkAsFailed(failedAt, "RabbitMQ indisponível");

        message.MarkAsProcessed(failedAt.AddMinutes(1));

        Assert.NotNull(message.ProcessedAtUtc);
        Assert.Null(message.NextAttemptAtUtc);
        Assert.Null(message.LastError);
        Assert.Equal(1, message.RetryCount);
    }

    [Fact]
    public void MarkAsFailed_IncrementsRetryAndSchedulesExponentialBackoff()
    {
        var message = CreateMessage();
        var failedAt = new DateTimeOffset(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);

        message.MarkAsFailed(failedAt, "RabbitMQ indisponível");

        Assert.Equal(1, message.RetryCount);
        Assert.Equal("RabbitMQ indisponível", message.LastError);
        Assert.Equal(failedAt.AddSeconds(2), message.NextAttemptAtUtc);
        Assert.Null(message.ProcessedAtUtc);
    }

    private static OutboxMessage CreateMessage()
    {
        var now = new DateTimeOffset(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);
        var entry = CashEntry.Create(
            Money.Create(100m), EntryType.Credit, "Venda de produto", now, now);
        var domainEvent = Assert.IsType<CashEntryCreatedDomainEvent>(Assert.Single(entry.DomainEvents));
        var mapper = new IntegrationEventMapper(new AccountingDateResolver("America/Sao_Paulo"));

        return Assert.IsType<OutboxMessage>(mapper.Map(domainEvent));
    }
}
