using CashFlow.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Erro de validação"),
            DomainException => (StatusCodes.Status400BadRequest, "Violação de regra de negócio"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Erro inesperado")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "An unhandled exception occurred.");
        }

        httpContext.Response.StatusCode = status;
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception switch
            {
                BadHttpRequestException =>
                    "O corpo da requisição contém um valor inválido. Verifique o formato da data de ocorrência.",
                _ when status == StatusCodes.Status500InternalServerError =>
                    "Ocorreu um erro inesperado.",
                _ => exception.Message
            },
            Type = $"https://httpstatuses.com/{status}"
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
