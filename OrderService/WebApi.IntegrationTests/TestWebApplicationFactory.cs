using System.Data.Common;
using Application.Abstractions.Data;
using Grpc.Net.Client;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OrderTrackingSystem.Grpc;
using Testcontainers.PostgreSql;

namespace WebApi.IntegrationTests;

[UsedImplicitly]
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings:Sql", dbContainer.GetConnectionString());
    }
    
    public OrderService.OrderServiceClient CreateGrpcClient()
    {
        var httpClient = CreateClient();
        
        var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        return new OrderService.OrderServiceClient(channel);
    }
    
    public async Task InitializeAsync()
    {
        await dbContainer.StartAsync();
        
        var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetService<IApplicationDbContext>()!;

        await new TestDataSeeder(dbContext).SeedAsync();
    }
    
    public new async Task DisposeAsync()
    {
        await dbContainer.StopAsync();
    }
}
