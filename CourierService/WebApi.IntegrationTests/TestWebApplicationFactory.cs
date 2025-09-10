using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
    
    public async Task InitializeAsync()
    {
        await dbContainer.StartAsync();
    }
    
    public new async Task DisposeAsync()
    {
        await dbContainer.StopAsync();
    }
}
