using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Infrastructure.Persistence.PostgreSQL.Configurations;

internal sealed class CashEntryConfiguration : IEntityTypeConfiguration<CashEntry>
{
    public void Configure(EntityTypeBuilder<CashEntry> builder)
    {
        builder.ToTable("cash_entries");

        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(entry => entry.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasColumnName("description")
            .HasMaxLength(CashEntry.MaximumDescriptionLength)
            .IsRequired();

        builder.Property(entry => entry.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(entry => entry.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.OwnsOne(entry => entry.Amount, money =>
        {
            money.Property(value => value.Value)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Navigation(entry => entry.Amount).IsRequired();
        builder.Ignore(entry => entry.DomainEvents);
        builder.HasIndex(entry => entry.OccurredAt)
            .HasDatabaseName("ix_cash_entries_occurred_at");
    }
}
