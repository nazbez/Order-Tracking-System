using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Data;
using Domain.CourierOrders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<CourierOrder> CourierOrders => Set<CourierOrder>();
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
