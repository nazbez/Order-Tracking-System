namespace WebApi.IntegrationTests;

public abstract class BaseTests(TestWebApplicationFactory factory) : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory = factory;
    
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await Factory.ResetDatabaseAsync();
}
