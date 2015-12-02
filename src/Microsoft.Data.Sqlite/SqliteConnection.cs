// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.IO;
#if NETCORE50
using System.Reflection;
using Microsoft.Data.Sqlite.Utilities;
#else
using Microsoft.Extensions.PlatformAbstractions;
#endif
using Microsoft.Data.Sqlite.Interop;

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

            var flags = Constants.SQLITE_OPEN_READWRITE | Constants.SQLITE_OPEN_CREATE;
            flags |= (ConnectionStringBuilder.Cache == SqliteConnectionCacheMode.Shared) ? Constants.SQLITE_OPEN_SHAREDCACHE : Constants.SQLITE_OPEN_PRIVATECACHE;

            var path = ConnectionStringBuilder.DataSource;

            if (!path.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
            {
                path = AdjustForRelativeDirectory(path);
            }

            var rc = NativeMethods.sqlite3_open_v2(path, out _db, flags, vfs: null);
            MarshalEx.ThrowExceptionForRC(rc, _db);
            SetState(ConnectionState.Open);

            SetFolders();
        }

        partial void SetFolders();

#if NETCORE50
        partial void SetFolders()
        {
            if (TemporaryFolderPath != null)
            {
                DbConnectionExtensions.ExecuteNonQuery(this, "PRAGMA temp_store_directory = '" + TemporaryFolderPath + "'");
            }
        }

        private readonly static string DefaultBasePath = AppData?.LocalFolder.Path;
        private readonly static string TemporaryFolderPath = AppData?.TemporaryFolder.Path;

        private static dynamic AppData
        {
            get
            {
                var appDataType = Type.GetType("Windows.Storage.ApplicationData, Windows, ContentType=WindowsRuntime", throwOnError: false);
                var appData = (dynamic)appDataType?.GetTypeInfo()
                    .GetDeclaredProperty("Current").GetMethod.Invoke(null, null);
                return appData;
            }
        }
#else
        private readonly static string DefaultBasePath = PlatformServices.Default.Application.ApplicationBasePath;
#endif

        internal static string AdjustForRelativeDirectory(string path)
        {
            if (DefaultBasePath == null)
            {
                return path;
            }

            if (path.StartsWith("file:"))
            {
#if NETCORE50
                // UWP cannot adjust for URI on UWP
                return path;
#else
                // is this already absolute?
                if (path.StartsWith("file:/"))
                {
                    return path;
                }
                var prefix = PlatformServices.Default.Runtime.OperatingSystem.Equals("Windows", StringComparison.OrdinalIgnoreCase) ? "file:///" : "file://";
                return prefix + DefaultBasePath + "/" + path.Replace("file:", "");
#endif
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            try
            {
                return Path.GetFullPath(Path.Combine(DefaultBasePath, path));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
            {
                return path;
            }
        }

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
