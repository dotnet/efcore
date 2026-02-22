---
name: cosmos-provider
description: 'EF Core Azure Cosmos DB provider, Cosmos query translation, Cosmos SQL generation, document storage, partition keys. Use when working on CosmosQueryableMethodTranslatingExpressionVisitor, CosmosClientWrapper, or Cosmos-specific features.'
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

## Other Key Files

| Area | Path |
|------|------|
| LINQ → Cosmos | `src/EFCore.Cosmos/Query/Internal/CosmosQueryableMethodTranslatingExpressionVisitor.cs` |
| Expression → Cosmos SQL | `src/EFCore.Cosmos/Query/Internal/CosmosSqlTranslatingExpressionVisitor.cs` |
| SQL generation | `src/EFCore.Cosmos/Query/Internal/CosmosQuerySqlGenerator.cs` |
| Compilation | `src/EFCore.Cosmos/Query/Internal/CosmosShapedQueryCompilingExpressionVisitor.cs` |
| SQL AST nodes | `src/EFCore.Cosmos/Query/Internal/Expressions/` |

## Validation

- Provider functional tests pass against a Cosmos emulator or live instance
