---
name: cosmos-provider
description: 'EF Core Azure Cosmos DB provider, Cosmos query translation, Cosmos SQL generation, document storage, partition keys. Use when working on CosmosQueryableMethodTranslatingExpressionVisitor, CosmosClientWrapper, or Cosmos-specific features.'
user-invokable: false
---

# Cosmos DB Provider

Non-relational provider with its own parallel query pipeline. Uses JSON for document materialization.

## When to Use

- Adding or modifying Cosmos query translation
- Working on document storage, partition key configuration, or `CosmosClientWrapper`
- Debugging Cosmos SQL generation differences from relational SQL

## Key Differences from Relational

- No migrations — use `EnsureCreated()`
- Documents as JSON — owned types become embedded objects
- Partition key configuration required for performance
- Limited query translation (more client evaluation)
- `ETag` for optimistic concurrency
- No cross-container joins

## Key Files

| Area | Path |
|------|------|
| LINQ → Cosmos | `src/EFCore.Cosmos/Query/Internal/CosmosQueryableMethodTranslatingExpressionVisitor.cs` |
| Expression → Cosmos SQL | `src/EFCore.Cosmos/Query/Internal/CosmosSqlTranslatingExpressionVisitor.cs` |
| SQL generation | `src/EFCore.Cosmos/Query/Internal/CosmosQuerySqlGenerator.cs` |
| Compilation | `src/EFCore.Cosmos/Query/Internal/CosmosShapedQueryCompilingExpressionVisitor.cs` |
| Cosmos SDK wrapper | `src/EFCore.Cosmos/Storage/Internal/CosmosClientWrapper.cs` |
| SQL AST nodes | `src/EFCore.Cosmos/Query/Internal/Expressions/` (~33 expression types) |

## Testing

Unit tests: `test/EFCore.Cosmos.Tests/`. Functional tests: `test/EFCore.Cosmos.FunctionalTests/`.

## Validation

- Provider functional tests pass against a Cosmos emulator or live instance
