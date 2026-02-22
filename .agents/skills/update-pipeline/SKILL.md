---
name: update-pipeline
description: 'EF Core SaveChanges, modification commands, command batching, update SQL generation, stored procedure updates. Use when working on the update pipeline, CommandBatchPreparer, UpdateSqlGenerator, or ModificationCommand.'
user-invokable: false
---

# Update Pipeline

Converts tracked entity changes into database INSERT/UPDATE/DELETE commands during `SaveChanges()`.

## When to Use

- Modifying how changes are batched or ordered
- Working on SQL generation for INSERT/UPDATE/DELETE
- Handling store-generated values (identity, computed columns)
- Debugging concurrency or transaction issues in SaveChanges

## Flow

`SaveChanges()` → ``DetectChanges()` → `IDatabase.SaveChanges()`
  → `UpdateAdapter` creates `IUpdateEntry` list
  → `CommandBatchPreparer.BatchCommands()`
    → `ModificationCommand` per entity (maps to table row), composed of `ColumnModification` (maps to column value)
    → Topological sort via Multigraph (FK dependency ordering)
    → Groups into `ModificationCommandBatch` (respects max batch size)
  → `UpdateSqlGenerator` generates SQL per batch
  → `BatchExecutor` executes all batches in a transaction
  → `StateManager.AcceptAllChanges()`

## Other Key Files

| Area | Path |
|------|------|
| Shared tables | `src/EFCore.Relational/Update/Internal/SharedTableEntryMap.cs` |

## Concurrency

Concurrency tokens → WHERE conditions on UPDATE/DELETE. `AffectedCountModificationCommandBatch` checks affected rows. Throws `DbUpdateConcurrencyException` on mismatch.

## Validation

- `SaveChanges()` returns expected affected row count
- Store-generated values propagate back to entities after INSERT/UPDATE
- `DbUpdateConcurrencyException` thrown when expected for stale data
