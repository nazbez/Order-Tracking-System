using System.Diagnostics.CodeAnalysis;
using Domain.CourierOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

[ExcludeFromCodeCoverage]
public sealed class CourierOrderConfiguration : BaseEntityConfiguration<CourierOrder, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CourierOrder> builder)
    {
        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.CourierId)
            .HasDefaultValue(null);
        
        builder.Property(o => o.UpdatedAt)
            .HasDefaultValue(null);
        
        builder.HasIndex(o => o.OrderId, "IX_CourierOrders_OrderId");
        builder.HasIndex(o => o.CustomerId, "IX_CourierOrders_CustomerId");
        builder.HasIndex(o => o.CourierId, "IX_CourierOrders_CourierId");
    }
}
