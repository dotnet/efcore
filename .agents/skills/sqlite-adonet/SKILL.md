---
name: sqlite-adonet
description: 'Microsoft.Data.Sqlite ADO.NET provider for SQLite. Use when working on SqliteConnection, SqliteCommand, SqliteDataReader, connection pooling, user-defined functions, or SQLite-specific ADO.NET functionality.'
user-invokable: false
---

# Microsoft.Data.Sqlite

Standalone ADO.NET provider in `src/Microsoft.Data.Sqlite.Core/`, independent of EF Core. Implements `System.Data.Common` abstractions.

## When to Use

- Working on SQLite connection, command, or data reader behavior
- Modifying connection pooling logic
- Adding or changing user-defined function registration
- Debugging SQLite-specific data type handling

## Key Classes

| Class | Purpose |
|-------|---------|
| `SqliteConnection` | Connection lifecycle, `CreateFunction`/`CreateAggregate` |
| `SqliteCommand` | SQL execution, prepared statements |
| `SqliteDataReader` | Result set reading |
| `SqliteConnectionPool` | Warm/cold pool stacks, prune timer (2-4 min interval) |
| `SqliteConnectionPoolGroup` | Groups pools by connection string |
| `SqliteBlob` | Streaming blob I/O via `sqlite3_blob_*` APIs |

## Notable Implementation Details

- Static constructor calls `SQLitePCL.Batteries_V2.Init()` reflectively
- Connection pooling is opt-in (`Pooling=True` in connection string)
- `CreateFunction()`/`CreateAggregate()` overloads generated from T4 templates (`.tt` files)
- Core implementation: `CreateFunctionCore<TState, TResult>` wraps .NET delegates into SQLitePCL callbacks
- No true async I/O — async methods are sync wrappers
- File-based or `Data Source=:memory:` for in-memory databases

## Testing

Tests in `test/Microsoft.Data.Sqlite.Tests/`.
