using Domain.CourierOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<CourierOrder> CourierOrders { get; }
    
    DatabaseFacade Database { get; }
    
    DbSet<T> Set<T>() where T : class; 
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
