---
name: update-pipeline
description: 'Implementation details for EF Core SaveChanges and the update pipeline. Use when changing CommandBatchPreparer, UpdateSqlGenerator, ModificationCommand, or related classes.'
user-invokable: false
---

# Update Pipeline

Converts tracked entity changes into database INSERT/UPDATE/DELETE commands during `SaveChanges()`.

## Flow

`SaveChanges()` → `DetectChanges()` → `IDatabase.SaveChanges()`
  → `UpdateAdapter` creates `IUpdateEntry` list
  → `CommandBatchPreparer.BatchCommands()`
    → `ModificationCommand` per entity (maps to table row), composed of `ColumnModification` (maps to column value)
    → Topological sort via Multigraph (FK dependency ordering)
    → Groups into `ModificationCommandBatch` (respects max batch size)
  → `UpdateSqlGenerator` generates SQL per batch
  → `BatchExecutor` executes all batches in a transaction
  → `StateManager.AcceptAllChanges()`

Other Key Files:
- `src/EFCore.Relational/Update/Internal/SharedTableEntryMap.cs` — manages entries mapped to the same row

## Concurrency

Concurrency tokens → WHERE conditions on UPDATE/DELETE. `AffectedCountModificationCommandBatch` checks affected rows. Throws `DbUpdateConcurrencyException` on mismatch.

## Validation

- `SaveChanges()` returns expected affected row count
- Store-generated values propagate back to entities after INSERT/UPDATE
- `DbUpdateConcurrencyException` thrown when expected for stale data
