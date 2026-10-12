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

## Command Lifecycle

`SqliteConnection` tracks associated commands as weak references. Closing an open connection disposes every live tracked command before closing the native connection; command disposal releases prepared statements and unregisters the command. Diagnose close/reopen reports from this ownership path and the `System.Data.Common` contract, not from EF Core provider abstractions.

Closing does not erase the command's managed configuration or connection reference. If the same command object is used after reopening its connection, execution prepares fresh native statements against the new handle. Add a regression test only when the reported behavior remains reachable after following this lifecycle.

## Transaction Completion

A failed `COMMIT` can leave the SQLite transaction active and retryable. Do not call `Complete()` or add transaction-level retries; `SqliteCommand` already retries busy/locked results through the connection's `ExecuteNonQuery` path.

Rollback during disposal must detach the managed transaction even when rollback throws, so keep `Complete()` in its `finally`. Test failed commit separately from failed `Dispose()`/`DisposeAsync()`: preserve the original exception and verify disposal permits a subsequent transaction.