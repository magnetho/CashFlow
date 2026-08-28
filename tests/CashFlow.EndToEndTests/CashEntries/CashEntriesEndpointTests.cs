using System.Net;
using System.Net.Http.Json;
using System.Text;
using CashFlow.Application.Abstractions.Persistence;
using CashFlow.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CashFlow.EndToEndTests.CashEntries;

public sealed class CashEntriesEndpointTests : IClassFixture<CashFlowApiFactory>
{
    private readonly HttpClient _client;

    public CashEntriesEndpointTests(CashFlowApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_WhenRequestIsValid_ReturnsCreatedWithEntryId()
    {
        var request = new
        {
            type = "credit",
            amount = 150.50m,
            description = "Product sale",
            occurredAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        };

        var response = await _client.PostAsJsonAsync("/api/v1/cash-entries", request);
        var body = await response.Content.ReadFromJsonAsync<CreateEntryResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal($"/api/v1/cash-entries/{body.Id}", response.Headers.Location?.ToString());

        var getResponse = await _client.GetAsync($"/api/v1/cash-entries/{body.Id}");
        var listResponse = await _client.GetAsync("/api/v1/cash-entries?page=1&pageSize=10&type=credit");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
    }

    [Fact]
    public async Task Post_WhenAmountIsInvalid_ReturnsProblemDetails()
    {
        var request = new
        {
            type = "debit",
            amount = 0m,
            description = "Supplier payment",
            occurredAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        };

        var response = await _client.PostAsJsonAsync("/api/v1/cash-entries", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Post_WhenOccurredAtIsMissing_ReturnsPortugueseValidationError()
    {
        var request = new
        {
            type = "credit",
            amount = 100m,
            description = "Venda sem data"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/cash-entries", request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("A data de ocorrência é obrigatória.", content);
    }

    [Fact]
    public async Task Post_WhenOccurredAtHasInvalidFormat_ReturnsBadRequest()
    {
        const string json =
            """{"type":"credit","amount":100,"description":"Venda inválida","occurredAt":"data-invalida"}""";

        var response = await _client.PostAsync(
            "/api/v1/cash-entries",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record CreateEntryResponse(Guid Id);
}

public sealed class CashFlowApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICashEntryRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<IDailyBalanceReadRepository>();
            services.AddSingleton<ICashEntryRepository, InMemoryCashEntryRepository>();
            services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();
            services.AddSingleton<IDailyBalanceReadRepository, StubDailyBalanceRepository>();
        });
    }
}

internal sealed class InMemoryCashEntryRepository : ICashEntryRepository
{
    private readonly List<CashEntry> _entries = [];

    public Task AddAsync(CashEntry cashEntry, CancellationToken cancellationToken = default)
    {
        _entries.Add(cashEntry);
        return Task.CompletedTask;
    }

    public Task<CashEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_entries.SingleOrDefault(entry => entry.Id == id));

    public Task<(IReadOnlyList<CashEntry> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        CashFlow.Domain.Enums.EntryType? type,
        CancellationToken cancellationToken = default)
    {
        var filtered = type.HasValue
            ? _entries.Where(entry => entry.Type == type.Value).ToArray()
            : _entries.ToArray();
        IReadOnlyList<CashEntry> pageItems = filtered
            .Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return Task.FromResult((pageItems, filtered.Length));
    }
}

internal sealed class NoOpUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(1);
}

internal sealed class StubDailyBalanceRepository : IDailyBalanceReadRepository
{
    private static readonly DateOnly ExistingDate = new(2026, 8, 27);

    public Task<DailyBalanceReadModel?> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        DailyBalanceReadModel? result = date == ExistingDate
            ? new DailyBalanceReadModel(
                date,
                300m,
                50m,
                250m,
                new DateTimeOffset(2026, 8, 27, 18, 0, 0, TimeSpan.Zero))
            : null;

        return Task.FromResult(result);
    }
}
