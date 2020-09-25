// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.Sqlite.Properties;
using Microsoft.Data.Sqlite.Utilities;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents a connection to a SQLite database.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/connection-strings">Connection Strings</seealso>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/async">Async Limitations</seealso>
    public partial class SqliteConnection : DbConnection
    {
        internal const string MainDatabaseName = "main";

        private const string DataDirectoryMacro = "|DataDirectory|";

        private readonly List<WeakReference<SqliteCommand>> _commands = new List<WeakReference<SqliteCommand>>();

        private Dictionary<string, (object state, strdelegate_collation collation)> _collations;

        private Dictionary<(string name, int arity), (int flags, object state, delegate_function_scalar func)> _functions;

        private Dictionary<(string name, int arity), (int flags, object state, delegate_function_aggregate_step func_step,
            delegate_function_aggregate_final func_final)> _aggregates;

        private HashSet<(string file, string proc)> _extensions;

        private string _connectionString = string.Empty;
        private ConnectionState _state;
        private sqlite3 _db;
        private bool _extensionsEnabled;

        static SqliteConnection()
            => BundleInitializer.Initialize();

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteConnection" /> class.
        /// </summary>
        public SqliteConnection()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteConnection" /> class.
        /// </summary>
        /// <param name="connectionString">The string used to open the connection.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/connection-strings">Connection Strings</seealso>
        /// <seealso cref="SqliteConnectionStringBuilder" />
        public SqliteConnection(string connectionString)
            => ConnectionString = connectionString;

        /// <summary>
        ///     Gets a handle to underlying database connection.
        /// </summary>
        /// <value>A handle to underlying database connection.</value>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/interop">Interoperability</seealso>
        public virtual sqlite3 Handle
            => _db;

        /// <summary>
        ///     Gets or sets a string used to open the connection.
        /// </summary>
        /// <value>A string used to open the connection.</value>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/connection-strings">Connection Strings</seealso>
        /// <seealso cref="SqliteConnectionStringBuilder" />
        public override string ConnectionString
        {
            get => _connectionString;
            set
            {
                if (State != ConnectionState.Closed)
                {
                    throw new InvalidOperationException(Resources.ConnectionStringRequiresClosedConnection);
                }

                _connectionString = value ?? string.Empty;
                ConnectionOptions = new SqliteConnectionStringBuilder(value);
            }
        }

        internal SqliteConnectionStringBuilder ConnectionOptions { get; set; }

        /// <summary>
        ///     Gets the name of the current database. Always 'main'.
        /// </summary>
        /// <value>The name of the current database.</value>
        public override string Database
            => MainDatabaseName;

        /// <summary>
        ///     Gets the path to the database file. Will be absolute for open connections.
        /// </summary>
        /// <value>The path to the database file.</value>
        public override string DataSource
        {
            get
            {
                string dataSource = null;
                if (State == ConnectionState.Open)
                {
                    dataSource = sqlite3_db_filename(_db, MainDatabaseName).utf8_to_string();
                }

                return dataSource ?? ConnectionOptions.DataSource;
            }
        }

        /// <summary>
        ///     Gets or sets the default <see cref="SqliteCommand.CommandTimeout" /> value for commands created using
        ///     this connection. This is also used for internal commands in methods like
        ///     <see cref="BeginTransaction()" />.
        /// </summary>
        /// <value>The default <see cref="SqliteCommand.CommandTimeout" /> value.</value>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
        public virtual int DefaultTimeout { get; set; } = 30;

        /// <summary>
        ///     Gets the version of SQLite used by the connection.
        /// </summary>
        /// <value>The version of SQLite used by the connection.</value>
        public override string ServerVersion
            => sqlite3_libversion().utf8_to_string();

        /// <summary>
        ///     Gets the current state of the connection.
        /// </summary>
        /// <value>The current state of the connection.</value>
        public override ConnectionState State
            => _state;

        /// <summary>
        ///     Gets the <see cref="DbProviderFactory" /> for this connection.
        /// </summary>
        /// <value>The <see cref="DbProviderFactory" />.</value>
        protected override DbProviderFactory DbProviderFactory
            => SqliteFactory.Instance;

        /// <summary>
        ///     Gets or sets the transaction currently being used by the connection, or null if none.
        /// </summary>
        /// <value>The transaction currently being used by the connection.</value>
        protected internal virtual SqliteTransaction Transaction { get; set; }

        /// <summary>
        ///     Opens a connection to the database using the value of <see cref="ConnectionString" />. If
        ///     <c>Mode=ReadWriteCreate</c> is used (the default) the file is created, if it doesn't already exist.
        /// </summary>
        /// <exception cref="SqliteException">A SQLite error occurs while opening the connection.</exception>
        public override void Open()
        {
            if (State == ConnectionState.Open)
            {
                return;
            }

            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException(Resources.OpenRequiresSetConnectionString);
            }

            var filename = ConnectionOptions.DataSource;
            var flags = 0;

            if (filename.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                flags |= SQLITE_OPEN_URI;
            }

            switch (ConnectionOptions.Mode)
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
                        ConnectionOptions.Mode == SqliteOpenMode.ReadWriteCreate,
                        "ConnectionOptions.Mode is not ReadWriteCreate");
                    flags |= SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE;
                    break;
            }

            switch (ConnectionOptions.Cache)
            {
                case SqliteCacheMode.Shared:
                    flags |= SQLITE_OPEN_SHAREDCACHE;
                    break;

                case SqliteCacheMode.Private:
                    flags |= SQLITE_OPEN_PRIVATECACHE;
                    break;

                default:
                    Debug.Assert(
                        ConnectionOptions.Cache == SqliteCacheMode.Default,
                        "ConnectionOptions.Cache is not Default.");
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

            _state = ConnectionState.Open;
            try
            {
                if (!string.IsNullOrEmpty(ConnectionOptions.Password))
                {
                    if (SQLitePCLExtensions.EncryptionSupported(out var libraryName) == false)
                    {
                        throw new InvalidOperationException(Resources.EncryptionNotSupported(libraryName));
                    }

                    // NB: SQLite doesn't support parameters in PRAGMA statements, so we escape the value using the
                    //     quote function before concatenating.
                    var quotedPassword = this.ExecuteScalar<string>(
                        "SELECT quote($password);",
                        new SqliteParameter("$password", ConnectionOptions.Password));
                    this.ExecuteNonQuery("PRAGMA key = " + quotedPassword + ";");
                }

                if (SQLitePCLExtensions.EncryptionSupported() != false)
                {
                    // NB: Forces decryption. Throws when the key is incorrect.
                    this.ExecuteNonQuery("SELECT COUNT(*) FROM sqlite_master;");
                }

                if (ConnectionOptions.ForeignKeys.HasValue)
                {
                    this.ExecuteNonQuery(
                        "PRAGMA foreign_keys = " + (ConnectionOptions.ForeignKeys.Value ? "1" : "0") + ";");
                }

                if (ConnectionOptions.RecursiveTriggers)
                {
                    this.ExecuteNonQuery("PRAGMA recursive_triggers = 1;");
                }

                if (_collations != null)
                {
                    foreach (var item in _collations)
                    {
                        rc = sqlite3_create_collation(_db, item.Key, item.Value.state, item.Value.collation);
                        SqliteException.ThrowExceptionForRC(rc, _db);
                    }
                }

                if (_functions != null)
                {
                    foreach (var item in _functions)
                    {
                        rc = sqlite3_create_function(_db, item.Key.name, item.Key.arity, item.Value.state, item.Value.func);
                        SqliteException.ThrowExceptionForRC(rc, _db);
                    }
                }

                if (_aggregates != null)
                {
                    foreach (var item in _aggregates)
                    {
                        rc = sqlite3_create_function(
                            _db, item.Key.name, item.Key.arity, item.Value.state, item.Value.func_step, item.Value.func_final);
                        SqliteException.ThrowExceptionForRC(rc, _db);
                    }
                }

                var extensionsEnabledForLoad = false;
                if (_extensions != null
                    && _extensions.Count != 0)
                {
                    rc = sqlite3_enable_load_extension(_db, 1);
                    SqliteException.ThrowExceptionForRC(rc, _db);
                    extensionsEnabledForLoad = true;

                    foreach (var item in _extensions)
                    {
                        LoadExtensionCore(item.file, item.proc);
                    }
                }

                if (_extensionsEnabled != extensionsEnabledForLoad)
                {
                    rc = sqlite3_enable_load_extension(_db, _extensionsEnabled ? 1 : 0);
                    SqliteException.ThrowExceptionForRC(rc, _db);
                }
            }
            catch
            {
                _db.Dispose();
                _db = null;

                _state = ConnectionState.Closed;

                throw;
            }

            OnStateChange(new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
        }

        /// <summary>
        ///     Closes the connection to the database. Open transactions are rolled back.
        /// </summary>
        public override void Close()
        {
            if (State != ConnectionState.Open)
            {
                return;
            }

            Transaction?.Dispose();

            for (var i = _commands.Count - 1; i >= 0; i--)
            {
                var reference = _commands[i];
                if (reference.TryGetTarget(out var command))
                {
                    // NB: Calls RemoveCommand()
                    command.Dispose();
                }
                else
                {
                    _commands.RemoveAt(i);
                }
            }

            Debug.Assert(_commands.Count == 0);

            _db.Dispose();
            _db = null;

            _state = ConnectionState.Closed;
            OnStateChange(new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));
        }

        /// <summary>
        ///     Releases any resources used by the connection and closes it.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
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
        ///     Creates a new command associated with the connection.
        /// </summary>
        /// <returns>The new command.</returns>
        /// <remarks>
        ///     The command's <see cref="SqliteCommand.Transaction" /> property will also be set to the current
        ///     transaction.
        /// </remarks>
        public new virtual SqliteCommand CreateCommand()
            => new SqliteCommand
            {
                Connection = this,
                CommandTimeout = DefaultTimeout,
                Transaction = Transaction
            };

        /// <summary>
        ///     Creates a new command associated with the connection.
        /// </summary>
        /// <returns>The new command.</returns>
        protected override DbCommand CreateDbCommand()
            => CreateCommand();

        internal void AddCommand(SqliteCommand command)
            => _commands.Add(new WeakReference<SqliteCommand>(command));

        internal void RemoveCommand(SqliteCommand command)
        {
            for (var i = _commands.Count - 1; i >= 0; i--)
            {
                if (_commands[i].TryGetTarget(out var item)
                    && item == command)
                {
                    _commands.RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///     Create custom collation.
        /// </summary>
        /// <param name="name">Name of the collation.</param>
        /// <param name="comparison">Method that compares two strings.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/collation">Collation</seealso>
        public virtual void CreateCollation(string name, Comparison<string> comparison)
            => CreateCollation(
                name, null, comparison != null ? (_, s1, s2) => comparison(s1, s2) : (Func<object, string, string, int>)null);

        /// <summary>
        ///     Create custom collation.
        /// </summary>
        /// <typeparam name="T">The type of the state object.</typeparam>
        /// <param name="name">Name of the collation.</param>
        /// <param name="state">State object passed to each invocation of the collation.</param>
        /// <param name="comparison">Method that compares two strings, using additional state.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/collation">Collation</seealso>
        public virtual void CreateCollation<T>(string name, T state, Func<T, string, string, int> comparison)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var collation = comparison != null ? (v, s1, s2) => comparison((T)v, s1, s2) : (strdelegate_collation)null;

            if (State == ConnectionState.Open)
            {
                var rc = sqlite3_create_collation(_db, name, state, collation);
                SqliteException.ThrowExceptionForRC(rc, _db);
            }

            _collations ??= new Dictionary<string, (object, strdelegate_collation)>(StringComparer.OrdinalIgnoreCase);
            _collations[name] = (state, collation);
        }

        /// <summary>
        ///     Begins a transaction on the connection.
        /// </summary>
        /// <returns>The transaction.</returns>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/transactions">Transactions</seealso>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
        public new virtual SqliteTransaction BeginTransaction()
            => BeginTransaction(IsolationLevel.Unspecified);

        /// <summary>
        ///     Begins a transaction on the connection.
        /// </summary>
        /// <param name="deferred">
        ///     <see langword="true" /> to defer the creation of the transaction.
        ///     This also causes transactions to upgrade from read transactions to write transactions as needed by their commands.
        /// </param>
        /// <returns>The transaction.</returns>
        /// <remarks>
        ///     Warning, commands inside a deferred transaction can fail if they cause the
        ///     transaction to be upgraded from a read transaction to a write transaction
        ///     but the database is locked. The application will need to retry the entire
        ///     transaction when this happens.
        /// </remarks>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/transactions">Transactions</seealso>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
        public virtual SqliteTransaction BeginTransaction(bool deferred)
            => BeginTransaction(IsolationLevel.Unspecified, deferred);

        /// <summary>
        ///     Begins a transaction on the connection.
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction.</param>
        /// <returns>The transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => BeginTransaction(isolationLevel);

        /// <summary>
        ///     Begins a transaction on the connection.
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction.</param>
        /// <returns>The transaction.</returns>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/transactions">Transactions</seealso>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
        public new virtual SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
            => BeginTransaction(isolationLevel, deferred: isolationLevel == IsolationLevel.ReadUncommitted);

        /// <summary>
        ///     Begins a transaction on the connection.
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction.</param>
        /// <param name="deferred">
        ///     <see langword="true" /> to defer the creation of the transaction.
        ///     This also causes transactions to upgrade from read transactions to write transactions as needed by their commands.
        /// </param>
        /// <returns>The transaction.</returns>
        /// <remarks>
        ///     Warning, commands inside a deferred transaction can fail if they cause the
        ///     transaction to be upgraded from a read transaction to a write transaction
        ///     but the database is locked. The application will need to retry the entire
        ///     transaction when this happens.
        /// </remarks>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/transactions">Transactions</seealso>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
        public virtual SqliteTransaction BeginTransaction(IsolationLevel isolationLevel, bool deferred)
        {
            if (State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.CallRequiresOpenConnection(nameof(BeginTransaction)));
            }

            if (Transaction != null)
            {
                throw new InvalidOperationException(Resources.ParallelTransactionsNotSupported);
            }

            return Transaction = new SqliteTransaction(this, isolationLevel, deferred);
        }

        /// <summary>
        ///     Changes the current database. Not supported.
        /// </summary>
        /// <param name="databaseName">The name of the database to use.</param>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override void ChangeDatabase(string databaseName)
            => throw new NotSupportedException();

        /// <summary>
        ///     Enables extension loading on the connection.
        /// </summary>
        /// <param name="enable"> <see langword="true" /> to enable; <see langword="false" /> to disable. </param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/extensions">Extensions</seealso>
        public virtual void EnableExtensions(bool enable = true)
        {
            if (State == ConnectionState.Open)
            {
                var rc = sqlite3_enable_load_extension(_db, enable ? 1 : 0);
                SqliteException.ThrowExceptionForRC(rc, _db);
            }

            _extensionsEnabled = enable;
        }

        /// <summary>
        ///     Loads a SQLite extension library.
        /// </summary>
        /// <param name="file">The shared library containing the extension.</param>
        /// <param name="proc">The entry point. If null, the default entry point is used.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/extensions">Extensions</seealso>
        public virtual void LoadExtension(string file, string proc = null)
        {
            if (State == ConnectionState.Open)
            {
                int rc;

                var extensionsEnabledForLoad = false;
                if (!_extensionsEnabled)
                {
                    rc = sqlite3_enable_load_extension(_db, 1);
                    SqliteException.ThrowExceptionForRC(rc, _db);
                    extensionsEnabledForLoad = true;
                }

                LoadExtensionCore(file, proc);

                if (extensionsEnabledForLoad)
                {
                    rc = sqlite3_enable_load_extension(_db, 0);
                    SqliteException.ThrowExceptionForRC(rc, _db);
                }
            }

            _extensions ??= new HashSet<(string, string)>();
            _extensions.Add((file, proc));
        }

        private void LoadExtensionCore(string file, string proc)
        {
            if (proc == null)
            {
                // NB: SQLitePCL.raw doesn't expose sqlite3_load_extension()
                this.ExecuteNonQuery(
                    "SELECT load_extension($file);",
                    new SqliteParameter("$file", file));
            }
            else
            {
                this.ExecuteNonQuery(
                    "SELECT load_extension($file, $proc);",
                    new SqliteParameter("$file", file),
                    new SqliteParameter("$proc", proc));
            }
        }

        /// <summary>
        ///     Backup of the connected database.
        /// </summary>
        /// <param name="destination">The destination of the backup.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/backup">Online Backup</seealso>
        public virtual void BackupDatabase(SqliteConnection destination)
            => BackupDatabase(destination, MainDatabaseName, MainDatabaseName);

        /// <summary>
        ///     Backup of the connected database.
        /// </summary>
        /// <param name="destination">The destination of the backup.</param>
        /// <param name="destinationName">The name of the destination database.</param>
        /// <param name="sourceName">The name of the source database.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/backup">Online Backup</seealso>
        public virtual void BackupDatabase(SqliteConnection destination, string destinationName, string sourceName)
        {
            if (State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.CallRequiresOpenConnection(nameof(BackupDatabase)));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var close = false;
            if (destination.State != ConnectionState.Open)
            {
                destination.Open();
                close = true;
            }

            try
            {
                using var backup = sqlite3_backup_init(destination._db, destinationName, _db, sourceName);
                int rc;
                if (backup.IsInvalid)
                {
                    rc = sqlite3_errcode(destination._db);
                    SqliteException.ThrowExceptionForRC(rc, destination._db);
                }

                rc = sqlite3_backup_step(backup, -1);
                SqliteException.ThrowExceptionForRC(rc, destination._db);
            }
            finally
            {
                if (close)
                {
                    destination.Close();
                }
            }
        }

        private void CreateFunctionCore<TState, TResult>(
            string name,
            int arity,
            TState state,
            Func<TState, SqliteValueReader, TResult> function,
            bool isDeterministic)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            delegate_function_scalar func = null;
            if (function != null)
            {
                func = (ctx, user_data, args) =>
                {
                    // TODO: Avoid allocation when niladic
                    var values = new SqliteParameterReader(name, args);

                    try
                    {
                        // TODO: Avoid closure by passing function via user_data
                        var result = function((TState)user_data, values);

                        new SqliteResultBinder(ctx, result).Bind();
                    }
                    catch (Exception ex)
                    {
                        sqlite3_result_error(ctx, ex.Message);

                        if (ex is SqliteException sqlEx)
                        {
                            // NB: This must be called after sqlite3_result_error()
                            sqlite3_result_error_code(ctx, sqlEx.SqliteErrorCode);
                        }
                    }
                };
            }

            var flags = isDeterministic ? SQLITE_DETERMINISTIC : 0;

            if (State == ConnectionState.Open)
            {
                var rc = sqlite3_create_function(
                    _db,
                    name,
                    arity,
                    flags,
                    state,
                    func);
                SqliteException.ThrowExceptionForRC(rc, _db);
            }

            _functions ??= new Dictionary<(string, int), (int, object, delegate_function_scalar)>(FunctionsKeyComparer.Instance);
            _functions[(name, arity)] = (flags, state, func);
        }

        private void CreateAggregateCore<TAccumulate, TResult>(
            string name,
            int arity,
            TAccumulate seed,
            Func<TAccumulate, SqliteValueReader, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector,
            bool isDeterministic)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            delegate_function_aggregate_step func_step = null;
            if (func != null)
            {
                func_step = (ctx, user_data, args) =>
                {
                    var context = (AggregateContext<TAccumulate>)user_data;
                    if (context.Exception != null)
                    {
                        return;
                    }

                    // TODO: Avoid allocation when niladic
                    var reader = new SqliteParameterReader(name, args);

                    try
                    {
                        // TODO: Avoid closure by passing func via user_data
                        // NB: No need to set ctx.state since we just mutate the instance
                        context.Accumulate = func(context.Accumulate, reader);
                    }
                    catch (Exception ex)
                    {
                        context.Exception = ex;
                    }
                };
            }

            delegate_function_aggregate_final func_final = null;
            if (resultSelector != null)
            {
                func_final = (ctx, user_data) =>
                {
                    var context = (AggregateContext<TAccumulate>)user_data;

                    if (context.Exception == null)
                    {
                        try
                        {
                            // TODO: Avoid closure by passing resultSelector via user_data
                            var result = resultSelector(context.Accumulate);

                            new SqliteResultBinder(ctx, result).Bind();
                        }
                        catch (Exception ex)
                        {
                            context.Exception = ex;
                        }
                    }

                    if (context.Exception != null)
                    {
                        sqlite3_result_error(ctx, context.Exception.Message);

                        if (context.Exception is SqliteException sqlEx)
                        {
                            // NB: This must be called after sqlite3_result_error()
                            sqlite3_result_error_code(ctx, sqlEx.SqliteErrorCode);
                        }
                    }
                };
            }

            var flags = isDeterministic ? SQLITE_DETERMINISTIC : 0;
            var state = new AggregateContext<TAccumulate>(seed);

            if (State == ConnectionState.Open)
            {
                var rc = sqlite3_create_function(
                    _db,
                    name,
                    arity,
                    flags,
                    state,
                    func_step,
                    func_final);
                SqliteException.ThrowExceptionForRC(rc, _db);
            }

            _aggregates ??=
                new Dictionary<(string, int), (int, object, delegate_function_aggregate_step, delegate_function_aggregate_final)>(
                    FunctionsKeyComparer.Instance);
            _aggregates[(name, arity)] = (flags, state, func_step, func_final);
        }

        private static Func<TState, SqliteValueReader, TResult> IfNotNull<TState, TResult>(
            object x,
            Func<TState, SqliteValueReader, TResult> value)
            => x != null ? value : null;

        private static object[] GetValues(SqliteValueReader reader)
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);

            return values;
        }

        private sealed class AggregateContext<T>
        {
            public AggregateContext(T seed)
                => Accumulate = seed;

            public T Accumulate { get; set; }
            public Exception Exception { get; set; }
        }

        private sealed class FunctionsKeyComparer : IEqualityComparer<(string name, int arity)>
        {
            public static readonly FunctionsKeyComparer Instance = new FunctionsKeyComparer();

            public bool Equals((string name, int arity) x, (string name, int arity) y)
                => StringComparer.OrdinalIgnoreCase.Equals(x.name, y.name)
                    && x.arity == y.arity;

            public int GetHashCode((string name, int arity) obj)
            {
                var nameHashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.name);
                var arityHashCode = obj.arity.GetHashCode();

                return ((int)(((uint)nameHashCode << 5) | ((uint)nameHashCode >> 27)) + nameHashCode) ^ arityHashCode;
            }
        }
    }
}
