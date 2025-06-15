using System.Diagnostics.CodeAnalysis;
using Domain.Entities;
using Infrastructure.Persistence.ValueConvertors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

[ExcludeFromCodeCoverage]
public sealed class OrderConfiguration : BaseEntityConfiguration<Order, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Status)
            .HasConversion<StatusConvertor>();
        
        builder.HasIndex(o => o.CustomerId, "IX_Orders_CustomerId");

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
