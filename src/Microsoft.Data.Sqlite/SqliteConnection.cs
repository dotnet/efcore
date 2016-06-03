// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.Sqlite.Interop;

#if !NET451
using Microsoft.Data.Sqlite.Utilities;
#endif

using static Microsoft.Data.Sqlite.Interop.Constants;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents a connection to a SQLite database.
    /// </summary>
    public partial class SqliteConnection : DbConnection
    {
        private const string MainDatabaseName = "main";

        private string _connectionString;
        private ConnectionState _state;
        private Sqlite3Handle _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteConnection" /> class.
        /// </summary>
        public SqliteConnection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteConnection" /> class.
        /// </summary>
        /// <param name="connectionString">The string used to open the connection.</param>
        /// <seealso cref="SqliteConnectionStringBuilder" />
        public SqliteConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        internal virtual Sqlite3Handle DbHandle
            => _db;

        /// <summary>
        /// Gets a handle to underlying database connection.
        /// </summary>
        /// <seealso href="http://sqlite.org/c3ref/sqlite3.html">Database Connection Handle</seealso>
        public virtual IntPtr Handle
            => _db?.DangerousGetHandle() ?? IntPtr.Zero;

        /// <summary>
        /// Gets or sets a string used to open the connection.
        /// </summary>
        /// <seealso cref="SqliteConnectionStringBuilder" />
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

        /// <summary>
        /// Gets the name of the current database. Always 'main'.
        /// </summary>
        public override string Database
            => MainDatabaseName;

        /// <summary>
        /// Gets the path to the database file. Will be absolute for open connections.
        /// </summary>
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
        /// Gets the version of SQLite used by the connection.
        /// </summary>
        public override string ServerVersion
            => NativeMethods.sqlite3_libversion();

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        public override ConnectionState State
            => _state;

        /// <summary>
        /// Gets or sets the transaction currently being used by the connection, or null if none.
        /// </summary>
        protected internal virtual SqliteTransaction Transaction { get; set; }

        private void SetState(ConnectionState value)
        {
            var originalState = _state;
            if (originalState != value)
            {
                _state = value;
                OnStateChange(new StateChangeEventArgs(originalState, value));
            }
        }

        /// <summary>
        /// Opens a connection to the database using the value of <see cref="ConnectionString" />.
        /// </summary>
        /// <exception cref="SqliteException">A SQLite error occurs while opening the connection.</exception>
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
        }

#if !NET451
        private static string BaseDirectory
            => Environment.GetEnvironmentVariable("ADONET_DATA_DIR")
                ?? ApplicationDataHelper.LocalFolderPath
                ?? AppContext.BaseDirectory;
#else
        private static string BaseDirectory
            => AppDomain.CurrentDomain.GetData("DataDirectory") as string
                ?? AppDomain.CurrentDomain.BaseDirectory;
#endif

        /// <summary>
        /// Closes the connection to the database. Open transactions are rolled back.
        /// </summary>
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

        /// <summary>
        /// Releases any resources used by the connection and closes it.
        /// </summary>
        /// <param name="disposing">
        /// true to release managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates a new command associated with the connection.
        /// </summary>
        /// <returns>The new command.</returns>
        /// <remarks>
        /// The command's <seealso cref="SqliteCommand.Transaction" /> property will also be set to the current
        /// transaction.
        /// </remarks>
        public new virtual SqliteCommand CreateCommand()
            => new SqliteCommand { Connection = this, Transaction = Transaction };

        /// <summary>
        /// Creates a new command associated with the connection.
        /// </summary>
        /// <returns>The new command.</returns>
        protected override DbCommand CreateDbCommand()
            => CreateCommand();

        /// <summary>
        /// Begins a transaction on the connection.
        /// </summary>
        /// <returns>The transaction.</returns>
        public new virtual SqliteTransaction BeginTransaction()
            => BeginTransaction(IsolationLevel.Unspecified);

        /// <summary>
        /// Begins a transaction on the connection.
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction.</param>
        /// <returns>The transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => BeginTransaction(isolationLevel);

        /// <summary>
        /// Begins a transaction on the connection.
        /// </summary>
        /// <param name="isolationLevel">
        /// The isolation level of the transaction.
        /// <para>Only <see cref="IsolationLevel.ReadUncommitted" /> and <see cref="IsolationLevel.Serializable" /> are
        /// supported.</para>
        /// </param>
        /// <returns>The transaction.</returns>
        public new virtual SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.CallRequiresOpenConnection("BeginTransaction"));
            }
            if (Transaction != null)
            {
                throw new InvalidOperationException(Strings.ParallelTransactionsNotSupported);
            }

            return Transaction = new SqliteTransaction(this, isolationLevel, SqliteCommand.DefaultCommandTimeout);
        }

        /// <summary>
        /// Changes the current database. Not supported.
        /// </summary>
        /// <param name="databaseName">The name of the database to use.</param>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Enables extension loading on the connection.
        /// </summary>
        /// <param name="enable">true to enable; false to disable</param>
        /// <seealso href="http://sqlite.org/loadext.html">Run-Time Loadable Extensions</seealso>
        public virtual void EnableExtensions(bool enable = true)
        {
            if (_db == null
                || _db.IsInvalid)
            {
                throw new InvalidOperationException(Strings.CallRequiresOpenConnection(nameof(EnableExtensions)));
            }

            var rc = NativeMethods.sqlite3_enable_load_extension(_db, enable ? 1 : 0);
            MarshalEx.ThrowExceptionForRC(rc, _db);
        }
    }
}
