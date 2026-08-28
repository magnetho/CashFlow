using CashFlow.Domain.ValueObjects;
using FluentValidation;

namespace CashFlow.Application.Entries.Commands.CreateEntry;

public sealed class CreateCashEntryCommandValidator : AbstractValidator<CreateCashEntryCommand>
{
    public CreateCashEntryCommandValidator(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        RuleFor(command => command.Amount)
            .GreaterThan(0)
            .WithMessage("O valor deve ser maior que zero.")
            .LessThanOrEqualTo(Money.MaximumAmount)
            .WithMessage($"O valor não pode exceder {Money.MaximumAmount}.")
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("O valor deve ter no máximo duas casas decimais.");

        RuleFor(command => command.Type)
            .IsInEnum()
            .WithMessage("O tipo do lançamento deve ser 'credit' ou 'debit'.");

        RuleFor(command => command.Description)
            .NotEmpty()
            .WithMessage("A descrição é obrigatória.")
            .Must(description => description is null ||
                description.Trim().Length >= Domain.Entities.CashEntry.MinimumDescriptionLength)
            .WithMessage(
                $"A descrição deve ter no mínimo {Domain.Entities.CashEntry.MinimumDescriptionLength} caracteres.")
            .Must(description => description is null ||
                description.Trim().Length <= Domain.Entities.CashEntry.MaximumDescriptionLength)
            .WithMessage(
                $"A descrição deve ter no máximo {Domain.Entities.CashEntry.MaximumDescriptionLength} caracteres.");

        RuleFor(command => command.OccurredAt)
            .NotNull()
            .WithMessage("A data de ocorrência é obrigatória.")
            .Must(occurredAt => !occurredAt.HasValue || occurredAt.Value <= timeProvider.GetUtcNow())
            .WithMessage("A data de ocorrência não pode estar no futuro.");
    }
}
