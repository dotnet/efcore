---
name: sqlite-adonet
description: 'Implementation details for the Microsoft.Data.Sqlite ADO.NET provider. Use when changing files under `src/Microsoft.Data.Sqlite.Core/`.'
user-invocable: false
---

# Microsoft.Data.Sqlite

Standalone ADO.NET provider in `src/Microsoft.Data.Sqlite.Core/`, independent of EF Core. Implements `System.Data.Common` abstractions.

## Notable Implementation Details

- Static constructor calls `SQLitePCL.Batteries_V2.Init()` reflectively
- `CreateFunction()`/`CreateAggregate()` overloads generated from T4 templates (`.tt` files)
- `DbConnection.Close()` closes the connection but does not dispose commands associated with it; a reusable `DbCommand` may remain associated and execute after that same connection is reopened. Keep connection-driven statement cleanup separate from `DbCommand.Dispose()`, which is the terminal command-lifetime operation.
