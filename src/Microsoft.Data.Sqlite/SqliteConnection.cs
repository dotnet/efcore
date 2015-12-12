// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.Sqlite.Interop;

#if NETCORE50
using System.Reflection;
using Microsoft.Data.Sqlite.Utilities;
#endif

using static Microsoft.Data.Sqlite.Interop.Constants;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents a connection with a SQLite database.
    /// </summary>
    public partial class SqliteConnection : DbConnection
    {
        private const string MainDatabaseName = "main";

        private string _connectionString;
        private ConnectionState _state;
        private Sqlite3Handle _db;

        public SqliteConnection()
        {
        }

        public SqliteConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        internal virtual Sqlite3Handle DbHandle => _db;

        /// <summary>
        /// Represents an unmanaged pointer to a sqlite3 database object. <see href="https://www.sqlite.org/c3ref/sqlite3.html">See SQLite.org for more documentation on proper usage of this object.</see>
        /// </summary>
        public virtual IntPtr Handle => _db?.DangerousGetHandle() ?? IntPtr.Zero;

        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (State != ConnectionState.Closed)
                {
                    throw new InvalidOperationException(Strings.ConnectionStringRequiresClosedConnection);
                }

                _connectionString = value;
                ConnectionStringBuilder = new SqliteConnectionStringBuilder(value);
            }
        }

        internal SqliteConnectionStringBuilder ConnectionStringBuilder { get; set; }

        public override string Database => MainDatabaseName;

        public override string DataSource
        {
            get
            {
                string dataSource = null;
                if (State == ConnectionState.Open)
                {
                    dataSource = VersionedMethods.GetFilename(_db, MainDatabaseName);
                }

                return dataSource ?? ConnectionStringBuilder.DataSource;
            }
        }

        /// <summary>
        /// Corresponds to the version of the SQLite library used by the connection.
        /// </summary>
        public override string ServerVersion => NativeMethods.sqlite3_libversion();
        public override ConnectionState State => _state;
        protected internal SqliteTransaction Transaction { get; set; }

        private void SetState(ConnectionState value)
        {
            var originalState = _state;
            if (originalState != value)
            {
                _state = value;
                OnStateChange(new StateChangeEventArgs(originalState, value));
            }
        }

        public override void Open()
        {
            if (State == ConnectionState.Open)
            {
                return;
            }
            if (ConnectionString == null)
            {
                throw new InvalidOperationException(Strings.OpenRequiresSetConnectionString);
            }



            var filename = ConnectionStringBuilder.DataSource;
            var flags = 0;

            if (filename.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                flags |= SQLITE_OPEN_URI;
            }

            switch (ConnectionStringBuilder.Mode)
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
                        ConnectionStringBuilder.Mode == SqliteOpenMode.ReadWriteCreate,
                        "ConnectionStringBuilder.Mode is not ReadWriteCreate");
                    flags |= SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE;
                    break;
            }

            switch (ConnectionStringBuilder.Cache)
            {
                case SqliteCacheMode.Shared:
                    flags |= SQLITE_OPEN_SHAREDCACHE;
                    break;

                case SqliteCacheMode.Private:
                    flags |= SQLITE_OPEN_PRIVATECACHE;
                    break;

                default:
                    Debug.Assert(
                        ConnectionStringBuilder.Cache == SqliteCacheMode.Default,
                        "ConnectionStringBuilder.Cache is not Default.");
                    break;
            }

            if ((flags & SQLITE_OPEN_URI) == 0
                && !filename.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
                && !Path.IsPathRooted(filename))
            {
                filename = Path.GetFullPath(Path.Combine(BaseDirectory, filename));
            }

            var rc = NativeMethods.sqlite3_open_v2(filename, out _db, flags, vfs: null);
            MarshalEx.ThrowExceptionForRC(rc, _db);

            SetState(ConnectionState.Open);

            OnOpened();
        }

        partial void OnOpened();

#if NETCORE50
        partial void OnOpened()
        {
            var appDataType = CurrentApplicationData?.GetType();
            var temporaryFolder = appDataType?.GetRuntimeProperty("TemporaryFolder").GetValue(CurrentApplicationData);
            var temporaryFolderPath = temporaryFolder?.GetType().GetRuntimeProperty("Path").GetValue(temporaryFolder) as string;
            if (temporaryFolderPath != null)
            {
                DbConnectionExtensions.ExecuteNonQuery(this, "PRAGMA temp_store_directory = '" + temporaryFolderPath + "';");
            }
        }

        private static object CurrentApplicationData
            => Type.GetType("Windows.Storage.ApplicationData, Windows, ContentType=WindowsRuntime")
                ?.GetRuntimeProperty("Current").GetValue(null);

        private static string BaseDirectory
        {
            get
            {
                var appDataType = CurrentApplicationData?.GetType();
                var localFolder = appDataType?.GetRuntimeProperty("LocalFolder").GetValue(CurrentApplicationData);
                return (localFolder?.GetType().GetRuntimeProperty("Path").GetValue(localFolder) as string)
                    ?? AppContext.BaseDirectory;
            }
        }
#elif NET451
        private static string BaseDirectory
            => AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY") as string
                ?? AppDomain.CurrentDomain.BaseDirectory;
#else
        private static string BaseDirectory
            => AppContext.BaseDirectory;
#endif

        public override void Close()
        {
            if (_db == null
                || _db.IsInvalid)
            {
                return;
            }

            Transaction?.Dispose();
            _db.Dispose();
            _db = null;
            SetState(ConnectionState.Closed);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }

            base.Dispose(disposing);
        }

        // NB: Other providers don't set Transaction
        public new virtual SqliteCommand CreateCommand() => new SqliteCommand { Connection = this, Transaction = Transaction };
        protected override DbCommand CreateDbCommand() => CreateCommand();
        public new virtual SqliteTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => BeginTransaction(isolationLevel);

        public new virtual SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("BeginTransaction"));
            }
            if (Transaction != null)
            {
                throw new InvalidOperationException(Strings.ParallelTransactionsNotSupported);
            }

            return Transaction = new SqliteTransaction(this, isolationLevel);
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        public virtual void EnableExtensions(bool enable = true)
        {
            if (_db == null
                || _db.IsInvalid)
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection(nameof(EnableExtensions)));
            }

            var rc = NativeMethods.sqlite3_enable_load_extension(_db, enable ? 1 : 0);
            MarshalEx.ThrowExceptionForRC(rc, _db);
        }
    }
}
