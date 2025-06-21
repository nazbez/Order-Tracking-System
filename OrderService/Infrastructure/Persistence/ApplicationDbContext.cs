using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Data;
using Domain.OrderItems;
using Domain.Orders;
using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
