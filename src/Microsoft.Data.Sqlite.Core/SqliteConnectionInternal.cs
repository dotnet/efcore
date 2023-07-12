// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteConnectionInternal
    {
        private const string DataDirectoryMacro = "|DataDirectory|";

        private readonly sqlite3 _db;
        private readonly WeakReference<SqliteConnection?> _outerConnection = new(null);

        private SqliteConnectionPool? _pool;
        private volatile bool _active;
        private volatile bool _canBePooled = true;

        public SqliteConnectionInternal(SqliteConnectionStringBuilder connectionOptions, SqliteConnectionPool? pool = null)
        {
            var filename = connectionOptions.DataSource;
            var flags = 0;

            if (filename.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                flags |= SQLITE_OPEN_URI;
            }

            switch (connectionOptions.Mode)
            {
                case SqliteOpenMode.ReadOnly:
                    flags |= SQLITE_OPEN_READONLY;
                    break;

                case SqliteOpenMode.ReadWrite:
                    flags |= SQLITE_OPEN_READWRITE;
                    break;

                case SqliteOpenMode.Memory:
                    flags |= SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE | SQLITE_OPEN_MEMORY;
                    if ((flags & SQLITE_OPEN_URI) == 0)
                    {
                        flags |= SQLITE_OPEN_URI;
                        filename = "file:" + filename;
                    }

                    break;

                default:
                    Debug.Assert(
                        connectionOptions.Mode == SqliteOpenMode.ReadWriteCreate,
                        "connectionOptions.Mode is not ReadWriteCreate");
                    flags |= SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE;
                    break;
            }

            switch (connectionOptions.Cache)
            {
                case SqliteCacheMode.Shared:
                    flags |= SQLITE_OPEN_SHAREDCACHE;
                    break;

                case SqliteCacheMode.Private:
                    flags |= SQLITE_OPEN_PRIVATECACHE;
                    break;

                default:
                    Debug.Assert(
                        connectionOptions.Cache == SqliteCacheMode.Default,
                        "connectionOptions.Cache is not Default.");
                    break;
            }

            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (!string.IsNullOrEmpty(dataDirectory)
                && (flags & SQLITE_OPEN_URI) == 0
                && !filename.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
            {
                if (filename.StartsWith(DataDirectoryMacro, StringComparison.InvariantCultureIgnoreCase))
                {
                    filename = Path.Combine(dataDirectory, filename.Substring(DataDirectoryMacro.Length));
                }
                else if (!Path.IsPathRooted(filename))
                {
                    filename = Path.Combine(dataDirectory, filename);
                }
            }

            var rc = sqlite3_open_v2(filename, out _db, flags, vfs: null);
            SqliteException.ThrowExceptionForRC(rc, _db);

            if (connectionOptions.Password.Length != 0)
            {
                if (SQLitePCLExtensions.EncryptionSupported(out var libraryName) == false)
                {
                    throw new InvalidOperationException(Resources.EncryptionNotSupported(libraryName));
                }

                // NB: SQLite doesn't support parameters in PRAGMA statements, so we escape the value using the
                //     quote function before concatenating.
                var quotedPassword = ExecuteScalar(
                    "SELECT quote($password);",
                    connectionOptions.Password,
                    connectionOptions.DefaultTimeout);
                ExecuteNonQuery(
                    "PRAGMA key = " + quotedPassword + ";",
                    connectionOptions.DefaultTimeout);

                if (SQLitePCLExtensions.EncryptionSupported() != false)
                {
                    // NB: Forces decryption. Throws when the key is incorrect.
                    ExecuteNonQuery(
                        "SELECT COUNT(*) FROM sqlite_master;",
                        connectionOptions.DefaultTimeout);
                }
            }

            if (connectionOptions.ForeignKeys.HasValue)
            {
                ExecuteNonQuery(
                    "PRAGMA foreign_keys = " + (connectionOptions.ForeignKeys.Value ? "1" : "0") + ";",
                    connectionOptions.DefaultTimeout);
            }

            if (connectionOptions.RecursiveTriggers)
            {
                ExecuteNonQuery(
                    "PRAGMA recursive_triggers = 1;",
                    connectionOptions.DefaultTimeout);
            }

            _pool = pool;
        }

        public bool Leaked
            => _active && !_outerConnection.TryGetTarget(out _);

        public bool CanBePooled
            => _canBePooled && !_outerConnection.TryGetTarget(out _);

        public sqlite3? Handle
            => _db;

        public void DoNotPool()
            => _canBePooled = false;

        public void Activate(SqliteConnection outerConnection)
        {
            _active = true;
            _outerConnection.SetTarget(outerConnection);
        }

        public void Close()
        {
            if (_pool != null)
            {
                _pool.Return(this);
            }
            else
            {
                Dispose();
            }
        }

        public void Deactivate()
        {
            if (_outerConnection.TryGetTarget(out var outerConnection))
            {
                outerConnection!.Deactivate();
            }

            _outerConnection.SetTarget(null);
            _active = false;
        }

        public void Dispose()
        {
            _db.Dispose();
            _pool = null;
        }

        private void ExecuteNonQuery(string sql, int timeout)
            => RetryWhileBusy(() => sqlite3_exec(_db, sql), timeout);

        private string ExecuteScalar(string sql, string p1, int timeout)
        {
            var timer = Stopwatch.StartNew();
            sqlite3_stmt stmt = null!;
            RetryWhileBusy(() => sqlite3_prepare_v2(_db, sql, out stmt), timeout, timer);
            try
            {
                sqlite3_bind_text(stmt, 1, p1);

                RetryWhileBusy(() => sqlite3_step(stmt), () => sqlite3_reset(stmt), timeout, timer);

                return sqlite3_column_text(stmt, 0).utf8_to_string();
            }
            finally
            {
                stmt.Dispose();
            }
        }

        private void RetryWhileBusy(Func<int> action, int timeout, Stopwatch? timer = null)
            => RetryWhileBusy(action, () => { }, timeout, timer);

        private void RetryWhileBusy(Func<int> action, Action reset, int timeout, Stopwatch? timer = null)
        {
            int rc;
            timer ??= Stopwatch.StartNew();

            while (IsBusy(rc = action()))
            {
                if (timeout != 0
                    && timer.ElapsedMilliseconds >= timeout * 1000L)
                {
                    break;
                }

                reset();

                Thread.Sleep(150);
            }

            SqliteException.ThrowExceptionForRC(rc, _db);
        }

        private static bool IsBusy(int rc)
            => rc is SQLITE_LOCKED or SQLITE_BUSY or SQLITE_LOCKED_SHAREDCACHE;
    }
}
