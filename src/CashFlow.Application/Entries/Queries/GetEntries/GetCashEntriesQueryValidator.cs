using FluentValidation;

namespace CashFlow.Application.Entries.Queries.GetEntries;

internal sealed class GetCashEntriesQueryValidator : AbstractValidator<GetCashEntriesQuery>
{
    public GetCashEntriesQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0)
            .WithMessage("A página deve ser maior que zero.");
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100)
            .WithMessage("O tamanho da página deve estar entre 1 e 100.");
    }
}
