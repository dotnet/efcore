// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
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

            _pool = pool;
        }

        public bool Leaked
            => _active && !_outerConnection.TryGetTarget(out _);

        public bool CanBePooled
            => _canBePooled && !_outerConnection.TryGetTarget(out _);

        public sqlite3? Handle
            => _db;

        public Stopwatch Timer { get; } = new ();

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
    }
}
