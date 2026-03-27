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
    public async Task SingleStep_Request_ReturnsTrackingStatus_WithSuccessTrace()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();
        WithAdminContext(client);

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s1", userId = "demo-user", message = "Where is order 123?" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJson(response);
        Assert.Contains("123", json.GetProperty("answer").GetString());
        Assert.Equal(1, json.GetProperty("trace").GetProperty("toolCallsCount").GetInt32());
        Assert.Equal("Success", json.GetProperty("trace").GetProperty("terminationReason").GetString());
    }

    [Fact]
    public async Task MultiStep_Request_UsesLatestThenTracking()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();
        WithAdminContext(client);

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s2", userId = "demo-user", message = "Find my latest order and tell me its status" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJson(response);
        Assert.Equal(2, json.GetProperty("toolCalls").GetArrayLength());
        Assert.Equal("Success", json.GetProperty("trace").GetProperty("terminationReason").GetString());
    }

    [Fact]
    public async Task Clarification_Request_ReturnsClarificationTerminationReason()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();
        WithAdminContext(client);

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s3", userId = "demo-user", message = "Where is my order?" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJson(response);
        Assert.True(json.GetProperty("requiresClarification").GetBoolean());
        Assert.Equal("ClarificationNeeded", json.GetProperty("trace").GetProperty("terminationReason").GetString());
    }

    [Fact]
    public async Task ForbiddenToolAccess_IsRejectedByPolicy()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();
        WithUserContext(client, "regular-user");

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s4", userId = "regular-user", message = "Find my latest order and tell me its status" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJson(response);
        Assert.Contains("not allowed", json.GetProperty("answer").GetString());
        Assert.Equal("Forbidden", json.GetProperty("trace").GetProperty("terminationReason").GetString());
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
        WithAdminContext(client);

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s5", userId = "demo-user", message = "test invalid" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJson(response);
        Assert.True(json.GetProperty("requiresClarification").GetBoolean());
        Assert.Equal("InvalidArguments", json.GetProperty("trace").GetProperty("terminationReason").GetString());
        Assert.Equal(1, json.GetProperty("trace").GetProperty("validationFailuresCount").GetInt32());
    }

    [Fact]
    public async Task ServiceUnavailable_ReturnsGracefulFallback()
    {
        await using var factory = new GatewayFactory(services =>
        {
            services.RemoveAll<IServiceAdapter>();
            services.AddSingleton<IServiceAdapter, FailingTrackingAdapter>();
            services.AddSingleton<IServiceAdapter, FakeOrderAdapter>();
            services.AddSingleton<IServiceAdapter, CourierServiceAdapter>();
        });
        using var client = factory.CreateClient();
        WithAdminContext(client);

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s6", userId = "demo-user", message = "Where is order 999?" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJson(response);
        Assert.Contains("unavailable", json.GetProperty("answer").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("DegradedFallback", json.GetProperty("trace").GetProperty("terminationReason").GetString());
    }

    [Fact]
    public async Task SessionMemory_FollowUp_UsesStoredOrderContext_AndTraceShowsMemoryUsage()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();
        WithAdminContext(client);

        var sessionId = "s7";

        var first = await client.PostAsJsonAsync("/api/query", new
        {
            sessionId,
            userId = "demo-user",
            message = "Find my latest order and tell me its status"
        });

        var firstJson = await ReadJson(first);
        Assert.Equal("Success", firstJson.GetProperty("trace").GetProperty("terminationReason").GetString());

        var storedSessionResponse = await client.GetAsync($"/api/sessions/{sessionId}");
        Assert.Equal(HttpStatusCode.OK, storedSessionResponse.StatusCode);
        var storedSession = await ReadJson(storedSessionResponse);
        var resolvedOrderId = storedSession.GetProperty("resolvedOrderId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(resolvedOrderId));

        var second = await client.PostAsJsonAsync("/api/query", new
        {
            sessionId,
            userId = "demo-user",
            message = "What is the delivery ETA?"
        });

        var secondJson = await ReadJson(second);
        Assert.Contains("Estimated delivery time", secondJson.GetProperty("answer").GetString());
        Assert.True(secondJson.GetProperty("trace").GetProperty("usedSessionMemory").GetBoolean());
        Assert.Equal("Success", secondJson.GetProperty("trace").GetProperty("terminationReason").GetString());

        var stepKinds = secondJson.GetProperty("trace").GetProperty("steps")
            .EnumerateArray()
            .Select(s => s.GetProperty("kind").GetString())
            .ToArray();

        Assert.Contains("memory", stepKinds);
        Assert.Contains("session", stepKinds);
    }

    [Fact]
    public async Task UnsupportedRequest_ReturnsUnsupportedTermination()
    {
        await using var factory = new GatewayFactory();
        using var client = factory.CreateClient();
        WithAdminContext(client);

        var response = await client.PostAsJsonAsync("/api/query", new { sessionId = "s8", userId = "demo-user", message = "Tell me a joke about logistics" });

        var json = await ReadJson(response);
        Assert.Equal("Unsupported", json.GetProperty("trace").GetProperty("terminationReason").GetString());
        Assert.Contains("Unsupported request", json.GetProperty("answer").GetString());
    }

    private static void WithAdminContext(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("x-user-role");
        client.DefaultRequestHeaders.Remove("x-user-id");
        client.DefaultRequestHeaders.Add("x-user-role", "admin");
        client.DefaultRequestHeaders.Add("x-user-id", "demo-user");
    }

    private static void WithUserContext(HttpClient client, string userId)
    {
        client.DefaultRequestHeaders.Remove("x-user-role");
        client.DefaultRequestHeaders.Remove("x-user-id");
        client.DefaultRequestHeaders.Add("x-user-role", "user");
        client.DefaultRequestHeaders.Add("x-user-id", userId);
    }

    private static async Task<JsonElement> ReadJson(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload;
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
            return Task.FromResult(new ToolCallPlan(false, null, null, new ToolCallRequest("get_tracking_status", args.RootElement.Clone()), false, "invalid_test"));
        }

        public Task<string> ComposeFinalAnswerAsync(UserQuery query, SessionContext session, IReadOnlyCollection<ToolCallResult> results, CancellationToken cancellationToken)
            => Task.FromResult("done");
    }
}
