using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntelligentGateway.Application;
using IntelligentGateway.Domain;
using IntelligentGateway.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace IntelligentGateway.Tests;

public sealed class QueryIntegrationTests
{
    [Fact]
    public async Task SingleStep_Request_ReturnsTrackingStatus()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s1", userId = "demo-user", message = "Where is order 123?", role = "admin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var answer = json.GetProperty("answer").GetString();
        Assert.Contains("123", answer);
    }

    [Fact]
    public async Task MultiStep_Request_UsesLatestThenTracking()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s2", userId = "demo-user", message = "Find my latest order and tell me its status", role = "admin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, json.GetProperty("toolCalls").GetArrayLength());
    }

    [Fact]
    public async Task Ambiguous_Request_AsksClarification()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s3", userId = "demo-user", message = "Where is my order?", role = "admin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("requiresClarification").GetBoolean());
    }

    [Fact]
    public async Task ForbiddenToolAccess_IsRejectedByPolicy()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s4", userId = "regular-user", message = "Find my latest order and tell me its status", role = "user" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var answer = json.GetProperty("answer").GetString();
        Assert.Contains("not allowed", answer);
    }

    [Fact]
    public async Task InvalidToolArguments_AreRejectedBySchemaValidation()
    {
        await using var factory = new GatewayFactory(services =>
        {
            services.RemoveAll<IChatOrchestrator>();
            services.AddSingleton<IChatOrchestrator, InvalidArgsOrchestrator>();
        });
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s5", userId = "demo-user", message = "test invalid", role = "admin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("requiresClarification").GetBoolean());
        Assert.Contains("invalid", json.GetProperty("answer").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ServiceUnavailable_ReturnsGracefulResponse()
    {
        await using var factory = new GatewayFactory(services =>
        {
            services.RemoveAll<IServiceAdapter>();
            services.AddSingleton<IServiceAdapter, FailingTrackingAdapter>();
            services.AddSingleton<IServiceAdapter, FakeOrderAdapter>();
            services.AddSingleton<IServiceAdapter, CourierServiceAdapter>();
        });
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s6", userId = "demo-user", message = "Where is order 999?", role = "admin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("unavailable", json.GetProperty("answer").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class GatewayFactory(Action<IServiceCollection>? overrides = null) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISessionStore>();
                services.AddSingleton<ISessionStore, InMemorySessionStore>();

                services.RemoveAll<IServiceAdapter>();
                services.AddSingleton<IServiceAdapter, FakeOrderAdapter>();
                services.AddSingleton<IServiceAdapter, FakeTrackingAdapter>();
                services.AddSingleton<IServiceAdapter, CourierServiceAdapter>();

                overrides?.Invoke(services);
            });
        }
    }

    private sealed class FakeOrderAdapter : IServiceAdapter
    {
        public string Key => "order-service";

        public Task<JsonDocument> ExecuteAsync(string toolId, string argumentsJson, CancellationToken cancellationToken)
        {
            using var args = JsonDocument.Parse(argumentsJson);
            if (toolId == "get_latest_user_order")
            {
                return Task.FromResult(JsonSerializer.SerializeToDocument(new { id = "order-200", customerId = "demo-user", status = "Processing" }));
            }

            var orderId = args.RootElement.GetProperty("orderId").GetString();
            return Task.FromResult(JsonSerializer.SerializeToDocument(new { id = orderId, customerId = "demo-user", status = "Processing" }));
        }
    }

    private sealed class FakeTrackingAdapter : IServiceAdapter
    {
        public string Key => "tracking-service";

        public Task<JsonDocument> ExecuteAsync(string toolId, string argumentsJson, CancellationToken cancellationToken)
        {
            using var args = JsonDocument.Parse(argumentsJson);
            var orderId = args.RootElement.GetProperty("orderId").GetString();
            return Task.FromResult(JsonSerializer.SerializeToDocument(new { orderId, status = "OutForDelivery" }));
        }
    }

    private sealed class FailingTrackingAdapter : IServiceAdapter
    {
        public string Key => "tracking-service";

        public Task<JsonDocument> ExecuteAsync(string toolId, string argumentsJson, CancellationToken cancellationToken)
            => throw new HttpRequestException("Tracking unavailable");
    }

    private sealed class InvalidArgsOrchestrator : IChatOrchestrator
    {
        public Task<ToolCallPlan> PlanAsync(UserQuery query, SessionContext session, IReadOnlyCollection<ToolDefinition> allowedTools, IReadOnlyCollection<ToolCallResult> priorResults, CancellationToken cancellationToken)
        {
            var args = JsonSerializer.SerializeToDocument(new { });
            return Task.FromResult(new ToolCallPlan(false, null, null, new ToolCallRequest("get_tracking_status", args.RootElement.Clone())));
        }

        public Task<string> ComposeFinalAnswerAsync(UserQuery query, SessionContext session, IReadOnlyCollection<ToolCallResult> results, CancellationToken cancellationToken)
            => Task.FromResult("done");
    }
}
