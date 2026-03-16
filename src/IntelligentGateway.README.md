# Intelligent Gateway Prototype (.NET 8)

## Prototype purpose
This service is a thesis-focused MVP for an **Intelligent API Gateway**.
It accepts natural language queries, chooses allowed tools, validates tool arguments via JSON Schema, executes microservice adapters, persists short session state, and returns a final answer with trace metadata.

## Architecture overview
Projects:
- `src/IntelligentGateway.Api` – HTTP endpoints, request pipeline, health, metrics wiring.
- `src/IntelligentGateway.Application` – orchestration loop contracts + `UserQueryHandler`.
- `src/IntelligentGateway.Domain` – core models (`ToolDefinition`, `ToolCallResult`, `SessionContext`, etc.).
- `src/IntelligentGateway.Infrastructure` – tool registry, policy, validators, adapters, session stores.
- `src/IntelligentGateway.Tests` – integration tests for key MVP scenarios.

## Request flow
1. `POST /api/query` receives `sessionId`, `userId`, `message`.
2. Gateway loads session context.
3. Tool policy filters allowed tools.
4. Chat orchestrator produces one of: clarification, final answer, tool call.
5. Gateway validates tool arguments against tool JSON Schema.
6. Gateway executes adapter with timeout/retry metadata.
7. Result is appended to context; bounded loop repeats (max 3 steps).
8. Gateway returns final answer + execution trace.

## Supported demo scenarios
- Single-step: `Where is order 123?`
- Multi-step: `Find my latest order and tell me its status`
- Clarification: `Where is my order?`
- Forbidden tool access rejection.
- Invalid argument rejection by schema validator.
- Service unavailable graceful degradation.

## Endpoints
- `POST /api/query`
- `GET /api/sessions/{sessionId}`
- `GET /api/tools`
- `GET /health`

## LLM technology choice
- Uses **Microsoft.Extensions.AI** abstractions (`IChatClient`) for compatibility.
- MVP currently uses a deterministic rule-based orchestrator implementation to keep execution bounded and reproducible.
- `NoOpChatClient` is provided for local run; replace with real OpenAI-compatible chat client in production experiments.

## Local run
```bash
# from repository root, when dotnet SDK is available
dotnet run --project src/IntelligentGateway.Api
```

## Known limitations
- `get_latest_user_order` currently uses a placeholder adapter response (`TODO`) until OrderService exposes list/filter endpoint.
- `get_delivery_eta` currently returns mock payload (`TODO`) until courier ETA endpoint is available.
- No external auth integration in MVP; role and user policy are request-driven.

## Mapping to thesis idea
This prototype demonstrates that an API Gateway can become an intelligent orchestration layer while staying **deterministic and policy-controlled**:
- NL interpretation is separated from execution.
- Tool selection is bounded and validated.
- Security/policy remains gateway-enforced.
- Session state supports small multi-step interactions.
- Trace metadata makes behavior inspectable and testable.
