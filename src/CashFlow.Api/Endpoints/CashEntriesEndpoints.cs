using CashFlow.Api.Contracts.CashEntries;
using CashFlow.Application.Entries.Commands.CreateEntry;
using CashFlow.Application.Entries.Queries;
using CashFlow.Application.Entries.Queries.GetEntries;
using CashFlow.Application.Entries.Queries.GetEntry;
using CashFlow.Domain.Enums;
using MediatR;

namespace CashFlow.Api.Endpoints;

public static class CashEntriesEndpoints
{
    public static IEndpointRouteBuilder MapCashEntriesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/cash-entries")
            .WithTags("Cash Entries");

        group.MapPost("/", CreateCashEntry)
            .WithName("CreateCashEntry")
            .Produces<CreateCashEntryResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetCashEntry)
            .WithName("GetCashEntry")
            .Produces<CashEntryResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetCashEntries)
            .WithName("GetCashEntries")
            .Produces<PagedCashEntriesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateCashEntry(
        CreateCashEntryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCashEntryCommand(
            request.Amount,
            request.Type,
            request.Description,
            request.OccurredAt);

        var entryId = await sender.Send(command, cancellationToken);
        var response = new CreateCashEntryResponse(entryId);

        return Results.Created($"/api/v1/cash-entries/{entryId}", response);
    }

    private static async Task<IResult> GetCashEntry(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var entry = await sender.Send(new GetCashEntryQuery(id), cancellationToken);
        return entry is null ? Results.NotFound() : Results.Ok(entry);
    }

    private static async Task<IResult> GetCashEntries(
        int page, int pageSize, string? type, ISender sender, CancellationToken cancellationToken)
    {
        EntryType? parsedType = null;
        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!Enum.TryParse<EntryType>(type, ignoreCase: true, out var value)
                || !Enum.IsDefined(value))
            {
                return Results.BadRequest(new { error = "O tipo deve ser 'credit' ou 'debit'." });
            }

            parsedType = value;
        }

        var result = await sender.Send(
            new GetCashEntriesQuery(page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize, parsedType),
            cancellationToken);
        return Results.Ok(result);
    }

    private sealed record CreateCashEntryResponse(Guid Id);
}
