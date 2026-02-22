---
name: migrations
description: 'EF Core migrations, migration scaffolding, MigrationsSqlGenerator, model diffing, migration operations, HistoryRepository, Migrator. Use when working on migrations add, database update, or migration SQL generation.'
user-invokable: false
---

# Migrations

## Pipeline

**Add migration**: `MigrationsScaffolder.ScaffoldMigration()` → `MigrationsModelDiffer.GetDifferences()` → list of `MigrationOperation` → `CSharpMigrationsGenerator` produces Up/Down/Snapshot code

**Apply migration**: `Migrator.MigrateAsync()` → reads `__EFMigrationsHistory` → per pending: `MigrationsSqlGenerator.Generate(operations)` → `MigrationCommandExecutor` executes

## Key Files

| Area | Path |
|------|------|
| Scaffolder | `src/EFCore.Design/Migrations/Design/MigrationsScaffolder.cs` |
| C# generator | `src/EFCore.Design/Migrations/Design/CSharpMigrationsGenerator.cs` |
| Operation generator | `src/EFCore.Design/Migrations/Design/CSharpMigrationOperationGenerator.cs` |
| Snapshot generator | `src/EFCore.Design/Migrations/Design/CSharpSnapshotGenerator.cs` |
| SQL generator | `src/EFCore.Relational/Migrations/MigrationsSqlGenerator.cs` |
| Model differ | `src/EFCore.Relational/Migrations/Internal/MigrationsModelDiffer.cs` |
| History | `src/EFCore.Relational/Migrations/HistoryRepository.cs` |
| Operations | `src/EFCore.Relational/Migrations/Operations/` |

Provider overrides: `SqlServerMigrationsSqlGenerator`, `SqliteMigrationsSqlGenerator`

## Testing

Migration operation tests: `test/EFCore.Relational.Tests/Migrations/`. Functional tests: `test/EFCore.{Provider}.FunctionalTests/Migrations/`. Model differ tests: `test/EFCore.Relational.Tests/Migrations/Internal/MigrationsModelDifferTest*.cs`.

## Validation

- Generated migration code compiles and produces correct SQL
- `dotnet ef migrations script` output matches expected DDL
