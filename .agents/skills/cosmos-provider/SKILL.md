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

- `CosmosTestEnvironment.InitializeAsync()` uses `Test__Cosmos__DefaultConnection` if set, otherwise probes `https://localhost:8081` before starting a Linux emulator testcontainer.
- Skip tests requiring unsupported features on the Linux emulator with ` [ConditionalFact(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]`.
