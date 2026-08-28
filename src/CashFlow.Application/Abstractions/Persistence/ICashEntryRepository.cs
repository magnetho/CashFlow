using CashFlow.Domain.Entities;
using CashFlow.Domain.Enums;

namespace CashFlow.Application.Abstractions.Persistence;

public interface ICashEntryRepository
{
    Task AddAsync(CashEntry cashEntry, CancellationToken cancellationToken = default);

    Task<CashEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<CashEntry> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        EntryType? type,
        CancellationToken cancellationToken = default);
}
