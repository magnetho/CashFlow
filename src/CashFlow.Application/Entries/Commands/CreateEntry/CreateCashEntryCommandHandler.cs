using CashFlow.Application.Abstractions.Persistence;
using CashFlow.Domain.Entities;
using CashFlow.Domain.ValueObjects;
using MediatR;

namespace CashFlow.Application.Entries.Commands.CreateEntry;

public sealed class CreateCashEntryCommandHandler(
    ICashEntryRepository cashEntryRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CreateCashEntryCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateCashEntryCommand request,
        CancellationToken cancellationToken)
    {
        var amount = Money.Create(request.Amount);
        var cashEntry = CashEntry.Create(
            amount,
            request.Type,
            request.Description,
            request.OccurredAt!.Value,
            timeProvider.GetUtcNow());

        await cashEntryRepository.AddAsync(cashEntry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return cashEntry.Id;
    }
}
