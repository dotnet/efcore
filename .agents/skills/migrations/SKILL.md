---
name: migrations
description: 'Implementation details for EF Core migrations. Use when changing MigrationsSqlGenerator, model diffing, migration operations, HistoryRepository, the Migrator or related classes.'
user-invocable: false
---

# Migrations

## Pipeline

**Add migration**: `MigrationsScaffolder.ScaffoldMigration()` → `MigrationsModelDiffer.GetDifferences()` → list of `MigrationOperation` → `CSharpMigrationsGenerator` and `CSharpSnapshotGenerator` produce Up/Down/Snapshot code

**Apply migration**: `Migrator.MigrateAsync()` → reads `__EFMigrationsHistory` → per pending: `MigrationsSqlGenerator.Generate(operations)` → `MigrationCommandExecutor` executes

## Model Snapshot

- Model snapshots use `typeof(Dictionary<string, object>)` (property bag format), not the actual CLR type. When examining the `ClrType` in a snapshot, don't assume it matches the real entity type.
- `SnapshotModelProcessor.Process()` is used at design-time to fixup older model snapshots for backward compatibility.
- `MigrationsModelDiffer` uses provider-agnostic structural comparison between relational models to determine what migration operations are necessary.

For an upgrade-only regression, derive a concrete old `ModelSnapshot`, process its model with `SnapshotModelProcessor.Process(..., resetVersion: true)`, and compare it with the equivalent current design-time model through `MigrationsModelDiffer.GetDifferences()`. Preserve the property-bag CLR types emitted by old snapshots and omit APIs that would explicitly configure metadata that was absent in that version. The regression succeeds only when the resulting operation list is empty.

Legacy snapshot normalization belongs in `SnapshotModelProcessor`, not in runtime migration application. Scope a compatibility branch to the exact released versions that could emit the affected snapshot shape; versions from before the feature existed should not be included merely because they are numerically older. Prefer the mutable metadata interface when it exposes the required setter rather than casting to a concrete metadata implementation. Do not use a missing configuration source as the test for missing historical snapshot metadata: old snapshots may already configure a value using the current default. For metadata whose semantics changed, use the snapshot's product version to restore the old invariant. Add focused coverage for every version family named by the gate, plus the first unaffected version.

## Testing

Migration operation tests: `test/EFCore.Relational.Tests/Migrations/`. Functional tests: `test/EFCore.{Provider}.FunctionalTests/Migrations/`. Model differ tests: `test/EFCore.Relational.Tests/Migrations/Internal/MigrationsModelDifferTest*.cs`.

To simulate a snapshot model use `ModelBuilder` calls without conventions.