using CashFlow.Application.Abstractions.Time;
using CashFlow.Application.Behaviors;
using CashFlow.Application.Time;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CashFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        string accountingTimeZoneId)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IAccountingDateResolver>(
            new AccountingDateResolver(accountingTimeZoneId));

        return services;
    }
}
