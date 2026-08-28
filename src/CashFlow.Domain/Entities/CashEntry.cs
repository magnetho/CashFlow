using CashFlow.Domain.Abstractions;
using CashFlow.Domain.Enums;
using CashFlow.Domain.Events;
using CashFlow.Domain.Exceptions;
using CashFlow.Domain.ValueObjects;

namespace CashFlow.Domain.Entities;

public sealed class CashEntry : AggregateRoot
{
    public const int MinimumDescriptionLength = 3;
    public const int MaximumDescriptionLength = 200;

    private CashEntry()
    {
        Amount = null!;
        Description = string.Empty;
    }

    private CashEntry(
        Guid id,
        Money amount,
        EntryType type,
        string description,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Amount = amount;
        Type = type;
        Description = description;
        OccurredAt = occurredAt.ToUniversalTime();
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Money Amount { get; private set; }

    public EntryType Type { get; private set; }

    public string Description { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static CashEntry Create(
        Money amount,
        EntryType type,
        string description,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (!Enum.IsDefined(type))
        {
            throw new InvalidCashEntryException(
                "O tipo do lançamento deve ser 'credit' ou 'debit'.");
        }

        var normalizedDescription = description?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedDescription))
        {
            throw new InvalidCashEntryException("A descrição é obrigatória.");
        }

        if (normalizedDescription.Length < MinimumDescriptionLength)
        {
            throw new InvalidCashEntryException(
                $"A descrição deve ter no mínimo {MinimumDescriptionLength} caracteres.");
        }

        if (normalizedDescription.Length > MaximumDescriptionLength)
        {
            throw new InvalidCashEntryException(
                $"A descrição deve ter no máximo {MaximumDescriptionLength} caracteres.");
        }

        var normalizedCreatedAtUtc = createdAtUtc.ToUniversalTime();

        if (occurredAt > normalizedCreatedAtUtc)
        {
            throw new InvalidCashEntryException("A data de ocorrência não pode estar no futuro.");
        }

        var entry = new CashEntry(
            Guid.NewGuid(),
            amount,
            type,
            normalizedDescription,
            occurredAt,
            normalizedCreatedAtUtc);

        entry.RaiseDomainEvent(new CashEntryCreatedDomainEvent(
            Guid.NewGuid(),
            entry.Id,
            entry.Amount,
            entry.Type,
            entry.Description,
            entry.OccurredAt,
            normalizedCreatedAtUtc));

        return entry;
    }
}
