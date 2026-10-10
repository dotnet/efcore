---
name: update-pipeline
description: 'Implementation details for EF Core SaveChanges and the update pipeline. Use when changing CommandBatchPreparer, UpdateSqlGenerator, ModificationCommand, or related classes.'
user-invocable: false
---

# Update Pipeline

Converts tracked entity changes into database INSERT/UPDATE/DELETE commands during `SaveChanges()`.

## Flow

`SaveChanges()` → `DetectChanges()` → `IDatabase.SaveChanges()` → `UpdateAdapter` creates `IUpdateEntry` list → `CommandBatchPreparer.BatchCommands()`
    → `ModificationCommand` per table row, composed of `ColumnModification` per column
    → `SharedTableEntryMap` is used to track entries mapped to the same row
    → Topological sort via Multigraph (FK dependency ordering)
    → Groups into `ModificationCommandBatch` (respects max batch size)
    → `UpdateSqlGenerator` generates SQL per batch → `BatchExecutor` executes all batches in a transaction → `StateManager.AcceptAllChanges()`

## Concurrency

Concurrency tokens → WHERE conditions on UPDATE/DELETE. `AffectedCountModificationCommandBatch` checks affected rows. Throws `DbUpdateConcurrencyException` on mismatch.

## Shared-Row Value Access

Command state describes the row operation, not every entry: an Added replacement with a Deleted `SharedIdentityEntry` can participate in a Modified command. Original-value access must use the original valued from the deleted shared entry, preserving the previous FK for dependency ordering.

## Validation

- `SaveChanges()` returns expected affected row count
- Store-generated values propagate back to entities after INSERT/UPDATE
- `DbUpdateConcurrencyException` thrown when expected for stale data
