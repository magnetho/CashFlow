using CashFlow.Application.Abstractions.Time;

namespace CashFlow.Application.Time;

public sealed class AccountingDateResolver : IAccountingDateResolver
{
    private readonly TimeZoneInfo _accountingTimeZone;

    public AccountingDateResolver(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("Accounting timezone must be configured.", nameof(timeZoneId));
        }

        _accountingTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    public DateOnly Resolve(DateTimeOffset instant)
    {
        var localInstant = TimeZoneInfo.ConvertTime(instant, _accountingTimeZone);
        return DateOnly.FromDateTime(localInstant.DateTime);
    }
}
