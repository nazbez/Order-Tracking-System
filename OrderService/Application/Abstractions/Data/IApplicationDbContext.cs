using Domain.OrderItems;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    
    DatabaseFacade Database { get; }
    
    DbSet<T> Set<T>() where T : class; 
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
