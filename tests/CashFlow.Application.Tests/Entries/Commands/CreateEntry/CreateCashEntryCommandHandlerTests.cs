using CashFlow.Application.Abstractions.Persistence;
using CashFlow.Application.Entries.Commands.CreateEntry;
using CashFlow.Application.Tests.TestDoubles;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using NSubstitute;

namespace CashFlow.Application.Tests.Entries.Commands.CreateEntry;

public sealed class CreateCashEntryCommandHandlerTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenCommandIsValid_PersistsEntryAndCommitsTransaction()
    {
        var repository = Substitute.For<ICashEntryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var timeProvider = new FixedTimeProvider(NowUtc);
        var cancellationToken = new CancellationTokenSource().Token;
        CashEntry? persistedEntry = null;

        repository
            .AddAsync(Arg.Do<CashEntry>(entry => persistedEntry = entry), cancellationToken)
            .Returns(Task.CompletedTask);

        var handler = new CreateCashEntryCommandHandler(repository, unitOfWork, timeProvider);
        var command = new CreateCashEntryCommand(
            150.50m,
            EntryType.Credit,
            "Product sale",
            NowUtc.AddHours(-1));

        var entryId = await handler.Handle(command, cancellationToken);

        Assert.NotNull(persistedEntry);
        Assert.Equal(entryId, persistedEntry.Id);
        Assert.Equal(command.Amount, persistedEntry.Amount.Value);
        Assert.Equal(command.Type, persistedEntry.Type);
        Assert.Equal(command.Description, persistedEntry.Description);
        Assert.Equal(command.OccurredAt, persistedEntry.OccurredAt);
        Assert.Equal(NowUtc, persistedEntry.CreatedAtUtc);

        await repository.Received(1).AddAsync(persistedEntry, cancellationToken);
        await unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_AddsEntryBeforeCommitting()
    {
        var repository = Substitute.For<ICashEntryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCashEntryCommandHandler(
            repository,
            unitOfWork,
            new FixedTimeProvider(NowUtc));

        await handler.Handle(
            new CreateCashEntryCommand(100m, EntryType.Debit, "Supplier payment", NowUtc),
            CancellationToken.None);

        Received.InOrder(() =>
        {
            repository.AddAsync(Arg.Any<CashEntry>(), CancellationToken.None);
            unitOfWork.SaveChangesAsync(CancellationToken.None);
        });
    }
}
