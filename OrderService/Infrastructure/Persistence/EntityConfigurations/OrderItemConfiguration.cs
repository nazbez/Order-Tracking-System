using System.Diagnostics.CodeAnalysis;
using Domain.OrderItems;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

[ExcludeFromCodeCoverage]
public sealed class OrderItemConfiguration : BaseEntityConfiguration<OrderItem, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(oi => oi.Price)
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(500);
    }
}
