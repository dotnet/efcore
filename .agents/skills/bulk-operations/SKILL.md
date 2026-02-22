---
name: bulk-operations
description: 'EF Core ExecuteUpdate, ExecuteDelete, set-based bulk CUD operations. Use when working on bulk update/delete LINQ translation or UpdateExpression/DeleteExpression SQL AST nodes.'
user-invokable: false
---

# Bulk Operations

`ExecuteUpdate`/`ExecuteDelete` translate LINQ to set-based SQL UPDATE/DELETE, bypassing change tracking. Return affected row count.

## When to Use

- Adding or fixing translation of `ExecuteUpdate`/`ExecuteDelete`
- Working on `UpdateExpression`/`DeleteExpression` SQL AST nodes
- Handling provider-specific UPDATE/DELETE syntax differences

## Key Files

- Translation: `RelationalQueryableMethodTranslatingExpressionVisitor.ExecuteUpdate.cs` / `.ExecuteDelete.cs`
- SQL AST: `UpdateExpression.cs`, `DeleteExpression.cs` in `src/EFCore.Relational/Query/SqlExpressions/`
- Public API: `EntityFrameworkQueryableExtensions` — `ExecuteDelete`, `ExecuteUpdate` + async
- Setters: `SetPropertyCalls.cs`, `UpdateSettersBuilder.cs`

## Unsupported Scenarios

These throw at translation time — know them before attempting changes:

| Scenario | Error |
|----------|-------|
| TPC (non-leaf types) | `ExecuteOperationOnTPC` |
| TPT (ExecuteDelete) | `ExecuteOperationOnTPT` |
| JSON-mapped owned entities | `ExecuteOperationOnOwnedJsonIsNotSupported` |
| JSON-mapped complex types | `ExecuteUpdateOverJsonIsNotSupported` |
| Complex types in subqueries | `ExecuteUpdateSubqueryNotSupportedOverComplexTypes` |
| Keyless entities | `ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator` |
| Entity splitting (Delete) | `ExecuteOperationOnEntitySplitting` |
| Table splitting (Delete) | `ExecuteDeleteOnTableSplitting` |

## Testing

Specification tests: `test/EFCore.Specification.Tests/BulkUpdates/NorthwindBulkUpdatesTestBase.cs`. Uses `AssertDelete(async, query, rowsAffectedCount)` / `AssertUpdate(...)`. Provider overrides in `test/EFCore.{Provider}.FunctionalTests/BulkUpdates/` with `AssertSql()` baselines.

## Validation

- New operations translate to correct SQL
- Unsupported scenarios throw at translation time, not at runtime
