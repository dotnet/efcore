---
name: update-pipeline
description: 'Implementation details for EF Core SaveChanges and the update pipeline. Use when changing CommandBatchPreparer, UpdateSqlGenerator, ModificationCommand, or related classes.'
user-invocable: false
---

# Update Pipeline

Converts tracked entity changes into database INSERT/UPDATE/DELETE commands during `SaveChanges()`.

## Flow

`SaveChanges()` → `DetectChanges()` → `IDatabase.SaveChanges()`
  → `UpdateAdapter` creates `IUpdateEntry` list
  → `CommandBatchPreparer.BatchCommands()`
    → `ModificationCommand` per table row, composed of `ColumnModification` per column
      → `SharedTableEntryMap` is used to track entries mapped to the same row
    → Topological sort via Multigraph (FK dependency ordering)
    → Groups into `ModificationCommandBatch` (respects max batch size)
  → `UpdateSqlGenerator` generates SQL per batch
  → `BatchExecutor` executes all batches in a transaction
  → `StateManager.AcceptAllChanges()`

For shared columns, `ColumnValuePropagator.TryPropagate` returns `false` for the writing entry; a null complex property must not suppress that write. Only skip propagation into a null complex property after identifying the writer.

## Concurrency

Concurrency tokens → WHERE conditions on UPDATE/DELETE. `AffectedCountModificationCommandBatch` checks affected rows. Throws `DbUpdateConcurrencyException` on mismatch.

## Validation

- `SaveChanges()` returns expected affected row count
- Store-generated values propagate back to entities after INSERT/UPDATE
- `DbUpdateConcurrencyException` thrown when expected for stale data
