---
name: cosmos-provider
description: 'Implementation details for the EF Core Azure Cosmos DB provider. Use when changing Cosmos-specific code.'
user-invokable: false
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
