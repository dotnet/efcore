---
name: sqlite-adonet
description: 'Implementation details for the Microsoft.Data.Sqlite ADO.NET provider. Use when changing files under `src/Microsoft.Data.Sqlite.Core/`.'
user-invokable: false
---

# Microsoft.Data.Sqlite

Standalone ADO.NET provider in `src/Microsoft.Data.Sqlite.Core/`, independent of EF Core. Implements `System.Data.Common` abstractions.

## Notable Implementation Details

- Static constructor calls `SQLitePCL.Batteries_V2.Init()` reflectively
- `CreateFunction()`/`CreateAggregate()` overloads generated from T4 templates (`.tt` files)
