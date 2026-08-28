using CashFlow.Application.Entries.Commands.CreateEntry;
using CashFlow.Application.Tests.TestDoubles;
using CashFlow.Domain.Enums;

namespace CashFlow.Application.Tests.Entries.Commands.CreateEntry;

public sealed class CreateCashEntryCommandValidatorTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);
    private readonly CreateCashEntryCommandValidator _validator = new(new FixedTimeProvider(NowUtc));

    [Fact]
    public async Task Validate_WhenCommandIsValid_ReturnsSuccess()
    {
        var command = new CreateCashEntryCommand(100.50m, EntryType.Credit, "Product sale", NowUtc);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("0", "O valor deve ser maior que zero.")]
    [InlineData("-1", "O valor deve ser maior que zero.")]
    [InlineData("10.001", "O valor deve ter no máximo duas casas decimais.")]
    public async Task Validate_WhenAmountIsInvalid_ReturnsExpectedError(
        string value,
        string expectedMessage)
    {
        var amount = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        var command = new CreateCashEntryCommand(amount, EntryType.Credit, "Product sale", NowUtc);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, failure => failure.ErrorMessage == expectedMessage);
    }

    [Fact]
    public async Task Validate_WhenTypeIsInvalid_ReturnsError()
    {
        var command = new CreateCashEntryCommand(100m, (EntryType)999, "Product sale", NowUtc);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, failure =>
            failure.ErrorMessage == "O tipo do lançamento deve ser 'credit' ou 'debit'.");
    }

    [Fact]
    public async Task Validate_WhenOccurredAtIsInFuture_ReturnsError()
    {
        var command = new CreateCashEntryCommand(
            100m, EntryType.Debit, "Supplier payment", NowUtc.AddTicks(1));

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, failure =>
            failure.ErrorMessage == "A data de ocorrência não pode estar no futuro.");
    }

    [Fact]
    public async Task Validate_WhenOccurredAtIsMissing_ReturnsRequiredError()
    {
        var command = new CreateCashEntryCommand(
            100m, EntryType.Credit, "Product sale", null);

        var result = await _validator.ValidateAsync(command);

        Assert.Contains(result.Errors, failure =>
            failure.ErrorMessage == "A data de ocorrência é obrigatória.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("AB")]
    public async Task Validate_WhenDescriptionIsInvalid_ReturnsError(string description)
    {
        var command = new CreateCashEntryCommand(100m, EntryType.Credit, description, NowUtc);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Description");
    }
}
