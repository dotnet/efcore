---
name: cosmos-provider
description: 'Implementation details for the EF Core Azure Cosmos DB provider. Use when changing Cosmos-specific code.'
user-invocable: false
---

# Cosmos DB Provider

Non-relational provider with its own parallel query pipeline. Uses JSON for document materialization.

## Key Differences from Relational

- No migrations — use `EnsureCreated()`
- Documents as JSON — owned and complex types become embedded objects
- Partition key configuration required for performance
- `ETag` for optimistic concurrency
- No cross-container joins

## Azure Cosmos DB Emulator for Tests

- `TestEnvironment.InitializeAsync()` auto-starts a `Testcontainers.CosmosDb` container when `Test__Cosmos__DefaultConnection` is not set. Set the env var to use an existing emulator instead.
- Skip tests requiring unsupported features on the Linux emulator with `[CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]`.
