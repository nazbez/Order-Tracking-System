using System.Data.Common;
using Application.Abstractions.Data;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace WebApi.IntegrationTests;

[UsedImplicitly]
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection dbConnection;
    
    private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings:Sql", dbContainer.GetConnectionString());
    }
    
    public async Task InitializeAsync()
    {
        await dbContainer.StartAsync();
        
        var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetService<IApplicationDbContext>()!;

        await new TestDataSeeder(dbContext).SeedAsync();
        
        dbConnection = new NpgsqlConnection(dbContainer.GetConnectionString());
        await dbConnection.OpenAsync();
    }
    
    public new async Task DisposeAsync()
    {
        await dbContainer.StopAsync();
    }
}
