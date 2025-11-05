---
name: cosmos-provider
description: 'Implementation details for the EF Core Azure Cosmos DB provider. Use when changing Cosmos-specific code.'
user-invocable: false
---

# Cosmos DB Provider

Non-relational provider with its own parallel query pipeline. Uses JSON for document materialization.

## When to Use

- Working on Cosmos SQL generation
- Working on document storage, partition key configuration, or `CosmosClientWrapper`

## Key Differences from Relational

- No migrations — use `EnsureCreated()`
- Documents as JSON — owned and complex types become embedded objects
- Partition key configuration required for performance
- `ETag` for optimistic concurrency
- No cross-container joins

## Azure Cosmos DB Emulator in Docker

Cosmos tests run on Helix via Docker sidecar containers:
- `eng/testing/run-cosmos-container.ps1`
- `eng/testing/run-cosmos-container.sh`

These scripts can be invoked locally for testing on machines that don't have the emulator installed, but have docker available.

The `Test__Cosmos__SkipConnectionCheck=true` env var is set to prevent tests from being skipped when the emulator failed to start.
