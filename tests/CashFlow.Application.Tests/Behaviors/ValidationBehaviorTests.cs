using CashFlow.Application.Behaviors;
using CashFlow.Application.Entries.Commands.CreateEntry;
using CashFlow.Application.Tests.TestDoubles;
using CashFlow.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CashFlow.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ThrowsAndDoesNotInvokeNextHandler()
    {
        var now = new DateTimeOffset(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);
        var validator = new CreateCashEntryCommandValidator(new FixedTimeProvider(now));
        var behavior = new ValidationBehavior<CreateCashEntryCommand, Guid>([validator]);
        var nextWasCalled = false;
        RequestHandlerDelegate<Guid> next = () =>
        {
            nextWasCalled = true;
            return Task.FromResult(Guid.NewGuid());
        };

        var action = () => behavior.Handle(
            new CreateCashEntryCommand(0, EntryType.Credit, "Product sale", now),
            next,
            CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(action);
        Assert.False(nextWasCalled);
    }

    [Fact]
    public async Task Handle_WhenNoValidatorsExist_InvokesNextHandler()
    {
        var expectedId = Guid.NewGuid();
        var behavior = new ValidationBehavior<CreateCashEntryCommand, Guid>([]);
        RequestHandlerDelegate<Guid> next = () => Task.FromResult(expectedId);

        var result = await behavior.Handle(
            new CreateCashEntryCommand(
                100m, EntryType.Credit, "Product sale", DateTimeOffset.UtcNow),
            next,
            CancellationToken.None);

        Assert.Equal(expectedId, result);
    }
}
