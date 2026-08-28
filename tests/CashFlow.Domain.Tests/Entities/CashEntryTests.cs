using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;
using CashFlow.Domain.Exceptions;
using CashFlow.Domain.ValueObjects;

namespace CashFlow.Domain.Tests.Entities;

public sealed class CashEntryTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(EntryType.Credit)]
    [InlineData(EntryType.Debit)]
    public void Create_WhenDataIsValid_CreatesEntry(EntryType type)
    {
        var amount = Money.Create(150.50m);
        var occurredAt = new DateTimeOffset(2026, 8, 27, 14, 30, 0, TimeSpan.FromHours(-3));

        var entry = CashEntry.Create(amount, type, "Sale at main store", occurredAt, NowUtc);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(amount, entry.Amount);
        Assert.Equal(type, entry.Type);
        Assert.Equal("Sale at main store", entry.Description);
        Assert.Equal(occurredAt.ToUniversalTime(), entry.OccurredAt);
        Assert.Equal(NowUtc, entry.CreatedAtUtc);
    }

    [Fact]
    public void Create_WhenOccurredAtIsRetroactive_CreatesEntry()
    {
        var occurredAt = NowUtc.AddDays(-30);

        var entry = CashEntry.Create(
            Money.Create(100m), EntryType.Credit, "Retroactive sale", occurredAt, NowUtc);

        Assert.Equal(occurredAt.ToUniversalTime(), entry.OccurredAt);
    }

    [Fact]
    public void Create_WhenOccurredAtIsInFuture_ThrowsInvalidCashEntryException()
    {
        var action = () => CashEntry.Create(
            Money.Create(100m),
            EntryType.Credit,
            "Future sale",
            NowUtc.AddSeconds(1),
            NowUtc);

        var exception = Assert.Throws<InvalidCashEntryException>(action);
        Assert.Equal("A data de ocorrência não pode estar no futuro.", exception.Message);
    }

    [Fact]
    public void Create_WhenEntryTypeIsInvalid_ThrowsInvalidCashEntryException()
    {
        var action = () => CashEntry.Create(
            Money.Create(100m), (EntryType)999, "Invalid type", NowUtc, NowUtc);

        var exception = Assert.Throws<InvalidCashEntryException>(action);
        Assert.Equal("O tipo do lançamento deve ser 'credit' ou 'debit'.", exception.Message);
    }

    [Fact]
    public void Create_WhenDataIsValid_RaisesCashEntryCreatedDomainEvent()
    {
        var entry = CashEntry.Create(
            Money.Create(100m), EntryType.Credit, "Product sale", NowUtc, NowUtc);

        var domainEvent = Assert.Single(entry.DomainEvents);
        var createdEvent = Assert.IsType<CashEntryCreatedDomainEvent>(domainEvent);

        Assert.NotEqual(Guid.Empty, createdEvent.EventId);
        Assert.Equal(entry.Id, createdEvent.EntryId);
        Assert.Equal(entry.Amount, createdEvent.Amount);
        Assert.Equal(entry.Type, createdEvent.Type);
        Assert.Equal(entry.Description, createdEvent.Description);
        Assert.Equal(entry.OccurredAt, createdEvent.OccurredAt);
        Assert.Equal(NowUtc, createdEvent.OccurredAtUtc);
    }

    [Fact]
    public void ClearDomainEvents_RemovesRaisedEvents()
    {
        var entry = CashEntry.Create(
            Money.Create(100m), EntryType.Debit, "Supplier payment", NowUtc, NowUtc);

        entry.ClearDomainEvents();

        Assert.Empty(entry.DomainEvents);
    }

    [Fact]
    public void Create_WhenDescriptionHasSurroundingSpaces_NormalizesDescription()
    {
        var entry = CashEntry.Create(
            Money.Create(100m), EntryType.Credit, "  Product sale  ", NowUtc, NowUtc);

        Assert.Equal("Product sale", entry.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenDescriptionIsEmpty_ThrowsInvalidCashEntryException(string description)
    {
        var action = () => CashEntry.Create(
            Money.Create(100m), EntryType.Credit, description, NowUtc, NowUtc);

        var exception = Assert.Throws<InvalidCashEntryException>(action);
        Assert.Equal("A descrição é obrigatória.", exception.Message);
    }

    [Fact]
    public void Create_WhenDescriptionIsTooShort_ThrowsInvalidCashEntryException()
    {
        var action = () => CashEntry.Create(
            Money.Create(100m), EntryType.Credit, "AB", NowUtc, NowUtc);

        var exception = Assert.Throws<InvalidCashEntryException>(action);
        Assert.Equal("A descrição deve ter no mínimo 3 caracteres.", exception.Message);
    }

    [Fact]
    public void Create_WhenDescriptionIsTooLong_ThrowsInvalidCashEntryException()
    {
        var action = () => CashEntry.Create(
            Money.Create(100m), EntryType.Credit, new string('A', 201), NowUtc, NowUtc);

        var exception = Assert.Throws<InvalidCashEntryException>(action);
        Assert.Equal("A descrição deve ter no máximo 200 caracteres.", exception.Message);
    }
}
