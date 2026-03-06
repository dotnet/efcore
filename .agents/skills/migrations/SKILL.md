---
name: migrations
description: 'Implementation details for EF Core migrations. Use when changing MigrationsSqlGenerator, model diffing, migration operations, HistoryRepository, the Migrator or related classes.'
user-invokable: false
---

# Migrations

## Pipeline

**Add migration**: `MigrationsScaffolder.ScaffoldMigration()` → `MigrationsModelDiffer.GetDifferences()` → list of `MigrationOperation` → `CSharpMigrationsGenerator` and `CSharpSnapshotGenerator` produce Up/Down/Snapshot code

**Apply migration**: `Migrator.MigrateAsync()` → reads `__EFMigrationsHistory` → per pending: `MigrationsSqlGenerator.Generate(operations)` → `MigrationCommandExecutor` executes

## Testing

Migration operation tests: `test/EFCore.Relational.Tests/Migrations/`. Functional tests: `test/EFCore.{Provider}.FunctionalTests/Migrations/`. Model differ tests: `test/EFCore.Relational.Tests/Migrations/Internal/MigrationsModelDifferTest*.cs`.