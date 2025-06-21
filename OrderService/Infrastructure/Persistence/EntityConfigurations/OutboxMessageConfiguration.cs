using System.Diagnostics.CodeAnalysis;
using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

[ExcludeFromCodeCoverage]
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Content)
            .HasMaxLength(5000)
            .HasColumnType("JSONB");

        builder.Property(e => e.Type)
            .HasMaxLength(100);

        builder.Property(e => e.Error)
            .IsRequired(false)
            .HasMaxLength(500);
    }
}
