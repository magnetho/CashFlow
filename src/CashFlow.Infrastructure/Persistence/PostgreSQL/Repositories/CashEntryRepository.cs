using CashFlow.Application.Abstractions.Persistence;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.Persistence.PostgreSQL.Repositories;

internal sealed class CashEntryRepository(CashFlowDbContext dbContext) : ICashEntryRepository
{
    public async Task AddAsync(
        CashEntry cashEntry,
        CancellationToken cancellationToken = default)
    {
        await dbContext.CashEntries.AddAsync(cashEntry, cancellationToken);
    }

    public Task<CashEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.CashEntries.AsNoTracking()
            .SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<CashEntry> Items, int TotalCount)> GetPageAsync(
        int page, int pageSize, EntryType? type,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.CashEntries.AsNoTracking();
        if (type.HasValue)
        {
            query = query.Where(entry => entry.Type == type.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
