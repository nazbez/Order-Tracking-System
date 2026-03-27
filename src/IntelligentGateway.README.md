# Intelligent Gateway Prototype (.NET 8, Deterministic MVP)

## Purpose (thesis evaluation focus)
This prototype demonstrates a **deterministic intelligent API Gateway** for order-tracking scenarios.
It is intentionally not a generic chatbot and not a production platform.

The MVP demonstrates:
- natural-language intent interpretation (rule-based),
- tool selection with allowlist/policy,
- JSON-schema argument validation,
- bounded tool execution loop,
- session-memory-assisted follow-up requests,
- explicit execution trace and termination reason.

## Architecture overview
Projects:
- `src/IntelligentGateway.Api` – HTTP API, request identity resolution, metrics, endpoints.
- `src/IntelligentGateway.Application` – orchestration loop (`UserQueryHandler`) and core contracts.
- `src/IntelligentGateway.Domain` – tool/session/trace models and termination reason enum.
- `src/IntelligentGateway.Infrastructure` – deterministic planner, tool registry, adapters, validation, policy, session stores.
- `src/IntelligentGateway.Tests` – integration tests for evaluation scenarios.

## Request flow (bounded and explicit)
1. `POST /api/query` receives message + optional session id.
2. Gateway resolves trusted request identity (headers-based MVP context).
3. Session is loaded (in-memory or Redis).
4. Allowed tools are derived by policy.
5. Deterministic planner returns one of:
   - clarification request,
   - tool call,
   - direct/unsupported final answer.
6. Tool args are validated by JSON Schema.
7. Tool executes through adapter with timeout/retry metadata.
8. Session state is updated with minimal structured facts (e.g., `resolvedOrderId`).
9. Loop continues up to max steps (3), then returns final answer and trace.

## Supported orchestration scenarios
- **Explicit order-id status**: “Where is order 123?”
- **Latest order status**: “Find my latest order and tell me its status”
- **Clarification when order info missing**: “Where is my order?”
- **Delivery ETA by order id**: “What is ETA for order 123?”
- **Follow-up in same session using memory**:
  - Turn 1 resolves order id,
  - Turn 2 asks “What is the delivery ETA?” and reuses session context.
- **Unsupported/ambiguous requests** return explicit unsupported response.

## Session memory behavior
The gateway stores minimal structured data:
- `resolvedOrderId`
- compact recent tool snapshots
- extracted entities (e.g., `orderId`)

Memory is used only when necessary to complete follow-up requests (e.g., status/ETA with omitted id).

## MVP security controls (lightweight)
No full auth server is used.
Instead, the API resolves an execution identity from trusted gateway context headers (`x-user-id`, `x-user-role`) via an internal resolver abstraction.
Policy checks use this resolved context and tool policy tags.

This is still MVP-level security, but cleaner than taking role directly from request body.

## Trace and metrics available for evaluation
Each response trace includes:
- orchestration step count,
- tool call count,
- clarification count,
- validation failure count,
- session read/update flags,
- whether session memory was used,
- termination reason (`Success`, `ClarificationNeeded`, `Forbidden`, `InvalidArguments`, `Unsupported`, `MaxStepsReached`, `DegradedFallback`),
- per-step metadata log.

Metrics include counters/histograms for request duration, tool calls, validation failures, clarifications, session reads/updates, and termination reason counts.

## Endpoints
- `POST /api/query`
- `GET /api/sessions/{sessionId}`
- `GET /api/tools`
- `GET /health`

## Notes on LLM compatibility
This MVP uses deterministic rule-based interpretation.
`Microsoft.Extensions.AI` (`IChatClient`) is already integrated as an abstraction point, so a future model-based interpreter can be added without redesigning the gateway control loop.

## Local run
```bash
# from repository root, when dotnet SDK is available
dotnet run --project src/IntelligentGateway.Api
```

## Current limitations
- `get_latest_user_order` adapter still uses a synthetic latest-order response until an OrderService list/filter endpoint is exposed.
- Delivery ETA is deterministic synthetic estimation (sufficient for evaluation, not real routing ETA).
- No authentication tokens, signatures, or external identity provider integration.
- Planner is intentionally rule-based and narrow in scope.

## Why this is thesis-appropriate
The prototype makes decision boundaries explicit and testable:
- planner decides intent + next action,
- gateway enforces policy/validation/execution,
- session memory is observable,
- outcomes are classified by termination reason.

This enables reproducible evaluation in an academic chapter while remaining small and understandable.
