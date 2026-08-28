using CashFlow.Application.Abstractions.Persistence;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Abstractions;
using CashFlow.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.Persistence.PostgreSQL;

public sealed class CashFlowDbContext(
    DbContextOptions<CashFlowDbContext> options,
    IIntegrationEventMapper integrationEventMapper)
    : DbContext(options), IUnitOfWork
{
    public DbSet<CashEntry> CashEntries => Set<CashEntry>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var aggregates = GetAggregatesWithDomainEvents();
        AddOutboxMessages(aggregates);

        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        ClearDomainEvents(aggregates);
        return result;
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var aggregates = GetAggregatesWithDomainEvents();
        AddOutboxMessages(aggregates);

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        ClearDomainEvents(aggregates);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashFlowDbContext).Assembly);
    }

    private AggregateRoot[] GetAggregatesWithDomainEvents() => ChangeTracker
        .Entries<AggregateRoot>()
        .Select(entry => entry.Entity)
        .Where(aggregate => aggregate.DomainEvents.Count > 0)
        .ToArray();

    private void AddOutboxMessages(IEnumerable<AggregateRoot> aggregates)
    {
        var trackedMessageIds = OutboxMessages.Local.Select(message => message.Id).ToHashSet();

        foreach (var domainEvent in aggregates.SelectMany(aggregate => aggregate.DomainEvents))
        {
            var outboxMessage = integrationEventMapper.Map(domainEvent);

            if (outboxMessage is not null && trackedMessageIds.Add(outboxMessage.Id))
            {
                OutboxMessages.Add(outboxMessage);
            }
        }
    }

    private static void ClearDomainEvents(IEnumerable<AggregateRoot> aggregates)
    {
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }
    }
}
