using Application.Abstractions.Data;
using Domain.Orders;

namespace WebApi.IntegrationTests;

public sealed class TestDataSeeder(IApplicationDbContext applicationDbContext)
{
    public static readonly Order[] Orders =
    [
        Order.Create("123 Main St", Guid.NewGuid()),
        Order.Create("124 Main St", Guid.NewGuid()),
        Order.Create("125 Main St", Guid.NewGuid()),
    ];
    
    public async Task SeedAsync()
    {
        await applicationDbContext.Orders.AddRangeAsync(Orders);
        await applicationDbContext.SaveChangesAsync(CancellationToken.None);
    }
}
