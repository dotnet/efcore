// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite;

/// <summary>
///     Represents a connection to a SQLite database.
/// </summary>
/// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/connection-strings">Connection Strings</seealso>
/// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/async">Async Limitations</seealso>
public partial class SqliteConnection : DbConnection
{
    internal const string MainDatabaseName = "main";

    private const int SQLITE_WIN32_DATA_DIRECTORY_TYPE = 1;
    private const int SQLITE_WIN32_TEMP_DIRECTORY_TYPE = 2;

    private readonly List<WeakReference<SqliteCommand>> _commands = [];

    private Dictionary<string, (object? state, strdelegate_collation? collation)>? _collations;

    private Dictionary<(string name, int arity), (int flags, object? state, delegate_function_scalar? func)>? _functions;

    private Dictionary<(string name, int arity), (int flags, object? state, delegate_function_aggregate_step? func_step,
        delegate_function_aggregate_final? func_final)>? _aggregates;

    private HashSet<(string file, string? proc)>? _extensions;

    private string _connectionString;
    private ConnectionState _state;
    private SqliteConnectionInternal? _innerConnection;
    private bool _extensionsEnabled;
    private int? _defaultTimeout;

    private static readonly StateChangeEventArgs _fromClosedToOpenEventArgs = new(ConnectionState.Closed, ConnectionState.Open);
    private static readonly StateChangeEventArgs _fromOpenToClosedEventArgs = new(ConnectionState.Open, ConnectionState.Closed);

    private static string[]? NativeDllSearchDirectories;

    static SqliteConnection()
    {
        Type.GetType("SQLitePCL.Batteries_V2, SQLitePCLRaw.batteries_v2")
            ?.GetRuntimeMethod("Init", Type.EmptyTypes)
            ?.Invoke(null, null);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Type? appDataType = null;
            Type? storageFolderType = null;
            try
            {
                appDataType = Type.GetType("Windows.Storage.ApplicationData, Windows, ContentType=WindowsRuntime")
                    ?? Type.GetType("Windows.Storage.ApplicationData, Microsoft.Windows.SDK.NET");

                storageFolderType = Type.GetType("Windows.Storage.StorageFolder, Windows, ContentType=WindowsRuntime")
                    ?? Type.GetType("Windows.Storage.StorageFolder, Microsoft.Windows.SDK.NET");
            }
            catch
            {
                // Ignore "Could not load assembly." or any type initialization error.
            }

            object? currentAppData = null;
            try
            {
                currentAppData = appDataType?.GetRuntimeProperty("Current")?.GetValue(null);
            }
            catch (TargetInvocationException ex) when (IsNoPackageIdentityException(ex.InnerException))
            {
                // Ignore the unpackaged-app "no package identity" case; ApplicationData.Current is unavailable there.
            }
            catch (InvalidOperationException ex) when (IsNoPackageIdentityException(ex))
            {
                // Ignore the unpackaged-app "no package identity" case; ApplicationData.Current is unavailable there.
            }
            catch (Exception ex) when (ex is NotImplementedException or NotSupportedException
                                           or TargetInvocationException { InnerException: NotImplementedException or NotSupportedException })
            {
                // Ignore when WinRT APIs aren't implemented or supported (e.g., running under Wine)
                // ApplicationData.Current is unavailable there.
            }

            if (currentAppData != null)
            {
                try
                {
                    var localFolder = appDataType?.GetRuntimeProperty("LocalFolder")?.GetValue(currentAppData);
                    var localFolderPath = (string?)storageFolderType?.GetRuntimeProperty("Path")?.GetValue(localFolder);
                    if (localFolderPath != null)
                    {
                        var rc = sqlite3_win32_set_directory(SQLITE_WIN32_DATA_DIRECTORY_TYPE, localFolderPath);
                        Debug.Assert(rc == SQLITE_OK);
                    }

                    var tempFolder = appDataType?.GetRuntimeProperty("TemporaryFolder")?.GetValue(currentAppData);
                    var tempFolderPath = (string?)storageFolderType?.GetRuntimeProperty("Path")?.GetValue(tempFolder);
                    if (tempFolderPath != null)
                    {
                        var rc = sqlite3_win32_set_directory(SQLITE_WIN32_TEMP_DIRECTORY_TYPE, tempFolderPath);
                        Debug.Assert(rc == SQLITE_OK);
                    }
                }
                catch (Exception ex) when (ex is NotImplementedException or NotSupportedException
                                               or TargetInvocationException { InnerException: NotImplementedException or NotSupportedException })
                {
                    // Ignore failures accessing LocalFolder/TemporaryFolder when WinRT isn't fully implemented.
                }
            }
        }
    }

    private static bool IsNoPackageIdentityException(Exception? ex)
        => (ex is InvalidOperationException or COMException)
            && (ex.HResult == unchecked((int)0x80073D54) // APPMODEL_ERROR_NO_PACKAGE
                || ex.HResult == unchecked((int)0x8000000D)); // E_ILLEGAL_STATE_CHANGE

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteConnection" /> class.
    /// </summary>
    public SqliteConnection()
        : this(null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteConnection" /> class.
    /// </summary>
    /// <param name="connectionString">The string used to open the connection.</param>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/connection-strings">Connection Strings</seealso>
    /// <seealso cref="SqliteConnectionStringBuilder" />
    public SqliteConnection(string? connectionString)
        => ConnectionString = connectionString;

    /// <summary>
    ///     Gets a handle to underlying database connection.
    /// </summary>
    /// <value>A handle to underlying database connection.</value>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/interop">Interoperability</seealso>
    public virtual sqlite3? Handle
        => _innerConnection?.Handle;

    /// <summary>
    ///     Gets or sets a string used to open the connection.
    /// </summary>
    /// <value>A string used to open the connection.</value>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/connection-strings">Connection Strings</seealso>
    /// <seealso cref="SqliteConnectionStringBuilder" />
    [AllowNull]
    public override string ConnectionString
    {
        get => _connectionString;

        [MemberNotNull(nameof(_connectionString), nameof(PoolGroup))]
        set
        {
            if (State != ConnectionState.Closed)
            {
                throw new InvalidOperationException(Resources.ConnectionStringRequiresClosedConnection);
            }

            _connectionString = value ?? string.Empty;

            PoolGroup = SqliteConnectionFactory.Instance.GetPoolGroup(_connectionString);
        }
    }

    internal SqliteConnectionPoolGroup PoolGroup { get; set; }

    internal SqliteConnectionStringBuilder ConnectionOptions
        => PoolGroup.ConnectionOptions;

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
            string? dataSource = null;
            if (State == ConnectionState.Open)
            {
                dataSource = sqlite3_db_filename(Handle, MainDatabaseName).utf8_to_string();
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
    public virtual int DefaultTimeout
    {
        get => _defaultTimeout ?? ConnectionOptions.DefaultTimeout;
        set => _defaultTimeout = value;
    }

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
    protected internal virtual SqliteTransaction? Transaction { get; set; }

    /// <summary>
    ///     Empties the connection pool.
    /// </summary>
    /// <remarks>Any open connections will not be returned to the pool when closed.</remarks>
    public static void ClearAllPools()
        => SqliteConnectionFactory.Instance.ClearPools();

    /// <summary>
    ///     Empties the connection pool associated with the connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <remarks>Any open connections will not be returned to the pool when closed.</remarks>
    public static void ClearPool(SqliteConnection connection)
        => connection.PoolGroup.Clear();

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

        _innerConnection = SqliteConnectionFactory.Instance.GetConnection(this);

        int rc;

        _state = ConnectionState.Open;
        try
        {
            if (ConnectionOptions.ForeignKeys.HasValue)
            {
                this.ExecuteNonQuery(
                    "PRAGMA foreign_keys = " + (ConnectionOptions.ForeignKeys.Value ? "1" : "0") + ";");
            }

            if (ConnectionOptions.Synchronous.HasValue)
            {
                this.ExecuteNonQuery(
                    "PRAGMA synchronous = " + (int)ConnectionOptions.Synchronous.Value + ";");
            }

            if (ConnectionOptions.RecursiveTriggers)
            {
                this.ExecuteNonQuery("PRAGMA recursive_triggers = 1;");
            }

            if (_collations != null)
            {
                foreach (var item in _collations)
                {
                    rc = sqlite3_create_collation(Handle, item.Key, item.Value.state, item.Value.collation);
                    SqliteException.ThrowExceptionForRC(rc, Handle);
                }
            }

            if (_functions != null)
            {
                foreach (var item in _functions)
                {
                    rc = sqlite3_create_function(
                        Handle, item.Key.name, item.Key.arity, item.Value.flags, item.Value.state, item.Value.func);
                    SqliteException.ThrowExceptionForRC(rc, Handle);
                }
            }

            if (_aggregates != null)
            {
                foreach (var item in _aggregates)
                {
                    rc = sqlite3_create_function(
                        Handle,
                        item.Key.name,
                        item.Key.arity,
                        item.Value.flags,
                        item.Value.state,
                        item.Value.func_step,
                        item.Value.func_final);
                    SqliteException.ThrowExceptionForRC(rc, Handle);
                }
            }

            if (_extensions != null
                && _extensions.Count != 0)
            {
                rc = sqlite3_db_config(Handle, SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION, 1, out _);
                SqliteException.ThrowExceptionForRC(rc, Handle);

                foreach (var (file, proc) in _extensions)
                {
                    LoadExtensionCore(file, proc);
                }
            }

            if (_extensionsEnabled)
            {
                rc = sqlite3_enable_load_extension(Handle, _extensionsEnabled ? 1 : 0);
                SqliteException.ThrowExceptionForRC(rc, Handle);
            }
        }
        catch
        {
            _innerConnection.Close();
            _innerConnection = null;

            _state = ConnectionState.Closed;

            throw;
        }

        OnStateChange(_fromClosedToOpenEventArgs);
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

        var commands = _commands;
        for (var i = commands.Count - 1; i >= 0; i--)
        {
            var reference = commands[i];
            if (reference.TryGetTarget(out var command))
            {
                // NB: Calls RemoveCommand()
                command.Dispose();
            }
        }

        _commands.Clear();
        _innerConnection!.Close();
        _innerConnection = null;

        _state = ConnectionState.Closed;
        OnStateChange(_fromOpenToClosedEventArgs);
    }

    internal void Deactivate()
    {
        int rc;

        if (_collations != null)
        {
            foreach (var item in _collations.Keys)
            {
                rc = sqlite3_create_collation(Handle, item, null, null);
                SqliteException.ThrowExceptionForRC(rc, Handle);
            }
        }

        if (_functions != null)
        {
            foreach (var (name, arity) in _functions.Keys)
            {
                rc = sqlite3_create_function(Handle, name, arity, null, null);
                SqliteException.ThrowExceptionForRC(rc, Handle);
            }
        }

        if (_aggregates != null)
        {
            foreach (var (name, arity) in _aggregates.Keys)
            {
                rc = sqlite3_create_function(
                    Handle, name, arity, null, null, null);
                SqliteException.ThrowExceptionForRC(rc, Handle);
            }
        }

        // TODO: Unload extensions (currently not supported by SQLite)
        if (_extensionsEnabled)
        {
            rc = sqlite3_enable_load_extension(Handle, 0);
            SqliteException.ThrowExceptionForRC(rc, Handle);
        }
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
        => new()
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
            var reference = _commands[i];
            if (reference != null
                && reference.TryGetTarget(out var item)
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
    public virtual void CreateCollation(string name, Comparison<string>? comparison)
        => CreateCollation(
            // ReSharper disable once RedundantCast
            name, null, comparison != null ? (_, s1, s2) => comparison(s1, s2) : (Func<object?, string, string, int>?)null);

    /// <summary>
    ///     Create custom collation.
    /// </summary>
    /// <typeparam name="T">The type of the state object.</typeparam>
    /// <param name="name">Name of the collation.</param>
    /// <param name="state">State object passed to each invocation of the collation.</param>
    /// <param name="comparison">Method that compares two strings, using additional state.</param>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/collation">Collation</seealso>
    public virtual void CreateCollation<T>(string name, T state, Func<T, string, string, int>? comparison)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var collation = comparison != null ? (v, s1, s2) => comparison((T)v, s1, s2) : (strdelegate_collation?)null;

        if (State == ConnectionState.Open)
        {
            var rc = sqlite3_create_collation(Handle, name, state, collation);
            SqliteException.ThrowExceptionForRC(rc, Handle);
        }

        _collations ??= [with(StringComparer.OrdinalIgnoreCase)];
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
        => State != ConnectionState.Open
            ? throw new InvalidOperationException(Resources.CallRequiresOpenConnection(nameof(BeginTransaction)))
            : Transaction != null
                ? throw new InvalidOperationException(Resources.ParallelTransactionsNotSupported)
                : (Transaction = new SqliteTransaction(this, isolationLevel, deferred));

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
    /// <param name="enable"><see langword="true" /> to enable; <see langword="false" /> to disable.</param>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/extensions">Extensions</seealso>
    public virtual void EnableExtensions(bool enable = true)
    {
        if (State == ConnectionState.Open)
        {
            var rc = sqlite3_enable_load_extension(Handle, enable ? 1 : 0);
            SqliteException.ThrowExceptionForRC(rc, Handle);
        }

        _extensionsEnabled = enable;
    }

    /// <summary>
    ///     Loads a SQLite extension library.
    /// </summary>
    /// <param name="file">The shared library containing the extension.</param>
    /// <param name="proc">The entry point. If null, the default entry point is used.</param>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/extensions">Extensions</seealso>
    public virtual void LoadExtension(string file, string? proc = null)
    {
        if (State == ConnectionState.Open)
        {
            int rc;

            if (!_extensionsEnabled)
            {
                rc = sqlite3_db_config(Handle, SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION, 1, out _);
                SqliteException.ThrowExceptionForRC(rc, Handle);
            }

            LoadExtensionCore(file, proc);
        }

        _extensions ??= [];
        _extensions.Add((file, proc));
    }

    private void LoadExtensionCore(string file, string? proc)
    {
        SqliteException? firstException = null;
        foreach (var path in GetLoadExtensionPaths(file))
        {
            var rc = sqlite3_load_extension(Handle, utf8z.FromString(path), utf8z.FromString(proc), out var errmsg);
            if (rc == SQLITE_OK)
            {
                return;
            }

            // We store the first exception so that error message looks more obvious if file appears in there
            firstException ??= new SqliteException(Resources.SqliteNativeError(rc, errmsg.utf8_to_string()), rc, rc);
        }

        if (firstException != null)
        {
            throw firstException;
        }
    }

    private static IEnumerable<string> GetLoadExtensionPaths(string file)
    {
        // we always try original input first
        yield return file;

        var dirName = Path.GetDirectoryName(file);

        // we don't try to guess directories for user, if they pass a path either absolute or relative - they're on their own
        if (!string.IsNullOrEmpty(dirName))
        {
            yield break;
        }

        var shouldTryAddingLibPrefix =
            !file.StartsWith("lib", StringComparison.Ordinal) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (shouldTryAddingLibPrefix)
        {
            yield return $"lib{file}";
        }

        NativeDllSearchDirectories ??= GetNativeDllSearchDirectories();

        foreach (var dir in NativeDllSearchDirectories)
        {
            yield return Path.Combine(dir, file);

            if (shouldTryAddingLibPrefix)
            {
                yield return Path.Combine(dir, $"lib{file}");
            }
        }
    }

    private static string[] GetNativeDllSearchDirectories()
    {
        var searchDirs = AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") as string;
        return string.IsNullOrEmpty(searchDirs)
            ? []
            : searchDirs!.Split([Path.PathSeparator], StringSplitOptions.RemoveEmptyEntries);
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
            using var backup = sqlite3_backup_init(destination.Handle, destinationName, Handle, sourceName);
            int rc;
            if (backup.IsInvalid)
            {
                rc = sqlite3_errcode(destination.Handle);
                SqliteException.ThrowExceptionForRC(rc, destination.Handle);
            }

            rc = sqlite3_backup_step(backup, -1);
            SqliteException.ThrowExceptionForRC(rc, destination.Handle);
        }
        finally
        {
            if (close)
            {
                destination.Close();
            }
        }
    }

    /// <summary>
    ///     Returns schema information for the data source of this connection.
    /// </summary>
    /// <remarks>
    ///     Implements the common ADO.NET schema collections documented for <see cref="DbConnection.GetSchema()" />.
    ///     The collections are provided so that Microsoft.Data.Sqlite conforms to the ADO.NET provider metadata contract.
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/dotnet/framework/data/adonet/getschema-and-schema-collections">ADO.NET GetSchema and schema collections</seealso>
    /// <seealso href="https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections">ADO.NET common schema collections</seealso>
    /// <returns>Schema information.</returns>
    public override DataTable GetSchema()
        => GetSchema(DbMetaDataCollectionNames.MetaDataCollections);

    /// <summary>
    ///     Returns schema information for the data source of this connection.
    /// </summary>
    /// <remarks>
    ///     Implements the common ADO.NET schema collections documented for <see cref="DbConnection.GetSchema(string)" />.
    ///     The collections are provided so that Microsoft.Data.Sqlite conforms to the ADO.NET provider metadata contract.
    ///     SQLite table, column, and index metadata is populated from the documented <c>sqlite_master</c> catalog and
    ///     table-valued <c>pragma_table_info</c> and <c>pragma_index_list</c> interfaces.
    ///     Views with unavailable dependencies and virtual tables whose modules are unavailable are omitted from the
    ///     Columns collection. On SQLite 3.37 and later, the <c>pragma_table_list</c> interface is used to exclude
    ///     virtual-table shadow tables; older SQLite versions use the <c>sqlite_master</c> fallback.
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/dotnet/framework/data/adonet/getschema-and-schema-collections">ADO.NET GetSchema and schema collections</seealso>
    /// <seealso href="https://learn.microsoft.com/dotnet/framework/data/adonet/common-schema-collections">ADO.NET common schema collections</seealso>
    /// <seealso href="https://sqlite.org/pragma.html">SQLite PRAGMA statements</seealso>
    /// <param name="collectionName">The name of the schema.</param>
    /// <returns>Schema information.</returns>
    public override DataTable GetSchema(string collectionName)
        => GetSchema(collectionName, []);

    /// <summary>
    ///     Returns schema information for the data source of this connection.
    /// </summary>
    /// <param name="collectionName">The name of the schema.</param>
    /// <param name="restrictionValues">The restrictions.</param>
    /// <returns>Schema information.</returns>
    public override DataTable GetSchema(string collectionName, string?[] restrictionValues)
    {
        if (restrictionValues is not null && restrictionValues.Length != 0)
        {
            throw new ArgumentException(Resources.TooManyRestrictions(collectionName));
        }

        if (string.Equals(collectionName, DbMetaDataCollectionNames.MetaDataCollections, StringComparison.OrdinalIgnoreCase))
        {
            return new DataTable(DbMetaDataCollectionNames.MetaDataCollections)
            {
                Columns =
                {
                    DbMetaDataColumnNames.CollectionName,
                    { DbMetaDataColumnNames.NumberOfRestrictions, typeof(int) },
                    { DbMetaDataColumnNames.NumberOfIdentifierParts, typeof(int) }
                },
                Rows =
                {
                    { DbMetaDataCollectionNames.DataSourceInformation, 0, 0 },
                    { DbMetaDataCollectionNames.DataTypes, 0, 0 },
                    { DbMetaDataCollectionNames.MetaDataCollections, 0, 0 },
                    { DbMetaDataCollectionNames.ReservedWords, 0, 0 },
                    { DbMetaDataCollectionNames.Restrictions, 0, 0 },
                    { "Tables", 0, 2 },
                    { "Columns", 0, 3 },
                    { "Indexes", 0, 3 }
                }
            };
        }

        if (string.Equals(collectionName, DbMetaDataCollectionNames.DataSourceInformation, StringComparison.OrdinalIgnoreCase))
        {
            var dataTable = new DataTable(DbMetaDataCollectionNames.DataSourceInformation);
            dataTable.Columns.Add(DbMetaDataColumnNames.CompositeIdentifierSeparatorPattern);
            dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductName);
            dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersion);
            dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersionNormalized);
            dataTable.Columns.Add(DbMetaDataColumnNames.GroupByBehavior, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.IdentifierCase, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.IdentifierPattern);
            dataTable.Columns.Add(DbMetaDataColumnNames.OrderByColumnsInSelect, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterMarkerFormat);
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterMarkerPattern);
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterNameMaxLength, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.ParameterNamePattern);
            dataTable.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierCase, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierPattern);
            dataTable.Columns.Add(DbMetaDataColumnNames.StatementSeparatorPattern);
            dataTable.Columns.Add(DbMetaDataColumnNames.StringLiteralPattern);
            dataTable.Columns.Add(DbMetaDataColumnNames.SupportedJoinOperators, typeof(int));

            var serverVersion = new Version(ServerVersion);
            var supportedJoinOperators = (int)GetSupportedJoinOperators(serverVersion);
            var normalizedVersion = GetNormalizedVersion(serverVersion);

            dataTable.Rows.Add(
                "\\.",
                "SQLite",
                ServerVersion,
                normalizedVersion,
                2,
                1,
                @"[\p{L}_\x80-\uFFFF][\p{L}\p{N}_$\x80-\uFFFF]*",
                false,
                "@{0}",
                @"(\?[0-9]+|\?|[@:$][\p{L}\p{N}_\x80-\uFFFF][\p{L}\p{N}_$\x80-\uFFFF]*(?:::[\p{L}\p{N}_$\x80-\uFFFF]*)*(?:\([^\s]*\))?)",
                DBNull.Value,
                @"^[\p{L}\p{N}_\x80-\uFFFF][\p{L}\p{N}_$\x80-\uFFFF]*(?:::[\p{L}\p{N}_$\x80-\uFFFF]*)*(?:\([^\s]*\))?$",
                1,
                "^(([^\"]|\"\")*)$",
                ";",
                "'(([^']|'')*)'",
                supportedJoinOperators);

            return dataTable;
        }

        if (string.Equals(collectionName, DbMetaDataCollectionNames.DataTypes, StringComparison.OrdinalIgnoreCase))
        {
            var dataTable = new DataTable(DbMetaDataCollectionNames.DataTypes);
            dataTable.Columns.Add(DbMetaDataColumnNames.TypeName);
            dataTable.Columns.Add(DbMetaDataColumnNames.ProviderDbType, typeof(int));
            dataTable.Columns.Add(DbMetaDataColumnNames.DataType);
            dataTable.Columns.Add(DbMetaDataColumnNames.ColumnSize, typeof(long));
            dataTable.Columns.Add(DbMetaDataColumnNames.CreateFormat);
            dataTable.Columns.Add(DbMetaDataColumnNames.CreateParameters);
            dataTable.Columns.Add(DbMetaDataColumnNames.IsAutoIncrementable, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsBestMatch, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsCaseSensitive, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsConcurrencyType, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsFixedLength, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsFixedPrecisionScale, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsLiteralSupported, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsLong, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsNullable, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsSearchable, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsSearchableWithLike, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.IsUnsigned, typeof(bool));
            dataTable.Columns.Add(DbMetaDataColumnNames.LiteralPrefix);
            dataTable.Columns.Add(DbMetaDataColumnNames.LiteralSuffix);
            dataTable.Columns.Add(DbMetaDataColumnNames.MaximumScale, typeof(short));
            dataTable.Columns.Add(DbMetaDataColumnNames.MinimumScale, typeof(short));

            dataTable.Rows.Add("INTEGER", 1, typeof(long).FullName, -1L, "INTEGER", null, true, true, false, false, false, false, true, false, true, true, true, false, null, null, null, null);
            dataTable.Rows.Add("REAL", 2, typeof(double).FullName, -1L, "REAL", null, false, true, false, false, false, false, true, false, true, true, true, false, null, null, null, null);
            dataTable.Rows.Add("TEXT", 3, typeof(string).FullName, -1L, "TEXT", null, false, true, true, false, false, false, true, true, true, true, true, DBNull.Value, "'", "'", null, null);
            dataTable.Rows.Add("BLOB", 4, typeof(byte[]).FullName, -1L, "BLOB", null, false, true, false, false, false, false, true, true, true, true, true, DBNull.Value, "X'", "'", null, null);
            return dataTable;
        }

        if (string.Equals(collectionName, DbMetaDataCollectionNames.Restrictions, StringComparison.OrdinalIgnoreCase))
        {
            return new DataTable(DbMetaDataCollectionNames.Restrictions)
            {
                Columns =
                {
                    DbMetaDataColumnNames.CollectionName,
                    "RestrictionName",
                    "RestrictionDefault",
                    { "RestrictionNumber", typeof(int) }
                }
            };
        }

        if (string.Equals(collectionName, DbMetaDataCollectionNames.ReservedWords, StringComparison.OrdinalIgnoreCase))
        {
            var dataTable = new DataTable(DbMetaDataCollectionNames.ReservedWords) { Columns = { DbMetaDataColumnNames.ReservedWord } };

            int rc;
            var count = sqlite3_keyword_count();
            for (var i = 0; i < count; i++)
            {
                rc = sqlite3_keyword_name(i, out var keyword);
                SqliteException.ThrowExceptionForRC(rc, null);

                dataTable.Rows.Add(keyword);
            }

            return dataTable;
        }

        if (string.Equals(collectionName, "Tables", StringComparison.OrdinalIgnoreCase))
        {
            var dataTable = new DataTable("Tables");
            dataTable.Columns.Add("TABLE_SCHEMA");
            dataTable.Columns.Add("TABLE_NAME");
            dataTable.Columns.Add("TABLE_TYPE");

            foreach (var (databaseName, tableName, tableType) in GetSchemaObjects())
            {
                dataTable.Rows.Add(databaseName, tableName, tableType is "table" or "virtual" ? "BASE TABLE" : "VIEW");
            }

            return dataTable;
        }

        if (string.Equals(collectionName, "Columns", StringComparison.OrdinalIgnoreCase))
        {
            var dataTable = new DataTable("Columns");
            dataTable.Columns.Add("TABLE_SCHEMA");
            dataTable.Columns.Add("TABLE_NAME");
            dataTable.Columns.Add("COLUMN_NAME");
            dataTable.Columns.Add("DATA_TYPE");
            dataTable.Columns.Add("IS_NULLABLE");
            dataTable.Columns.Add("COLUMN_DEFAULT");
            dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));

            var schemaObjects = GetSchemaObjects();
            var serverVersion = new Version(ServerVersion);
            var useTableList = IsTableListSupported(serverVersion);
            var useTableXInfo = IsTableXInfoSupported(serverVersion);
            foreach (var databaseName in GetDatabaseNames())
            {
                using var command = CreateCommand();
                command.CommandText = useTableList && useTableXInfo
                    ? """
                      SELECT m.name, p.name, p.type, p.[notnull], p.dflt_value, p.cid, p.pk, m.wr,
                          EXISTS (SELECT 1 FROM pragma_index_list(m.name, $schema) i WHERE i.origin = 'pk')
                      FROM pragma_table_list m
                      JOIN pragma_table_xinfo(m.name, $schema) p ON true
                      WHERE m.schema = $schema AND m.type = 'table'
                          AND m.name NOT LIKE 'sqlite\_%' ESCAPE '\'
                          AND p.hidden IN (0, 2, 3)
                      """
                    : useTableList
                    ? """
                      SELECT m.name, p.name, p.type, p.[notnull], p.dflt_value, p.cid, p.pk, m.wr,
                          EXISTS (SELECT 1 FROM pragma_index_list(m.name, $schema) i WHERE i.origin = 'pk')
                      FROM pragma_table_list m
                      JOIN pragma_table_info(m.name, $schema) p ON true
                      WHERE m.schema = $schema AND m.type = 'table'
                          AND m.name NOT LIKE 'sqlite\_%' ESCAPE '\'
                      """
                    : useTableXInfo
                    ? $"""
                      SELECT m.name, p.name, p.type, p.[notnull], p.dflt_value, p.cid, p.pk,
                          CASE WHEN m.sql LIKE '%WITHOUT ROWID%' THEN 1 ELSE 0 END,
                          EXISTS (SELECT 1 FROM pragma_index_list(m.name, $schema) i WHERE i.origin = 'pk')
                      FROM {QuoteIdentifier(databaseName)}.sqlite_master m
                      JOIN pragma_table_xinfo(m.name, $schema) p ON true
                      WHERE m.type = 'table'
                          AND m.name NOT LIKE 'sqlite\_%' ESCAPE '\'
                          AND m.sql NOT LIKE 'CREATE VIRTUAL%'
                          AND p.hidden IN (0, 2, 3)
                      """
                    : $"""
                      SELECT m.name, p.name, p.type, p.[notnull], p.dflt_value, p.cid, p.pk,
                          CASE WHEN m.sql LIKE '%WITHOUT ROWID%' THEN 1 ELSE 0 END,
                          EXISTS (SELECT 1 FROM pragma_index_list(m.name, $schema) i WHERE i.origin = 'pk')
                      FROM {QuoteIdentifier(databaseName)}.sqlite_master m
                      JOIN pragma_table_info(m.name, $schema) p ON true
                      WHERE m.type = 'table'
                          AND m.name NOT LIKE 'sqlite\_%' ESCAPE '\'
                          AND m.sql NOT LIKE 'CREATE VIRTUAL%'
                      """;
                command.Parameters.AddWithValue("$schema", databaseName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dataTable.Rows.Add(
                            databaseName,
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.IsDBNull(2) ? DBNull.Value : reader.GetString(2),
                            reader.GetInt64(3) != 0
                            || (reader.GetInt64(6) != 0
                                && reader.GetInt64(7) == 0
                                && reader.GetInt64(8) == 0
                                && string.Equals(reader.GetString(2), "INTEGER", StringComparison.OrdinalIgnoreCase))
                                ? "NO"
                                : "YES",
                            reader.IsDBNull(4) ? DBNull.Value : reader.GetString(4),
                            reader.GetInt64(5));
                    }
                }

                foreach (var (_, tableName, tableType) in schemaObjects.Where(o => o.DatabaseName == databaseName && (o.TableType is "view" or "virtual")))
                {
                    using var viewCommand = CreateCommand();
                    viewCommand.CommandText = $"SELECT name, type, [notnull], dflt_value, cid FROM pragma_table_{(useTableXInfo ? "xinfo" : "info")}($table, $schema)" + (useTableXInfo ? " WHERE hidden IN (0, 2, 3)" : null);
                    viewCommand.Parameters.AddWithValue("$table", tableName);
                    viewCommand.Parameters.AddWithValue("$schema", databaseName);

                    try
                    {
                        using var reader = viewCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            dataTable.Rows.Add(
                                databaseName,
                                tableName,
                                reader.GetString(0),
                                reader.IsDBNull(1) ? DBNull.Value : reader.GetString(1),
                                reader.GetInt64(2) == 0 ? "YES" : "NO",
                                reader.IsDBNull(3) ? DBNull.Value : reader.GetString(3),
                                reader.GetInt64(4));
                        }
                    }
                    catch (SqliteException ex) when (IsUnavailableSchemaObjectException(ex))
                    {
                        // Ignore views whose dependencies are unavailable.
                    }
                }
            }

            return dataTable;
        }

        if (string.Equals(collectionName, "Indexes", StringComparison.OrdinalIgnoreCase))
        {
            var dataTable = new DataTable("Indexes");
            dataTable.Columns.Add("TABLE_SCHEMA");
            dataTable.Columns.Add("TABLE_NAME");
            dataTable.Columns.Add("INDEX_NAME");
            dataTable.Columns.Add("IS_UNIQUE", typeof(bool));
            dataTable.Columns.Add("ORIGIN");

            var useTableList = IsTableListSupported(new Version(ServerVersion));
            foreach (var databaseName in GetDatabaseNames())
            {
                using var command = CreateCommand();
                command.CommandText = useTableList
                    ? """
                      SELECT i.name, i.[unique], i.origin, m.name
                      FROM pragma_table_list m
                      JOIN pragma_index_list(m.name, $schema) i ON true
                      WHERE m.schema = $schema AND m.type = 'table'
                          AND m.name NOT LIKE 'sqlite\_%' ESCAPE '\'
                      """
                    : $"""
                      SELECT i.name, i.[unique], i.origin, m.name
                      FROM {QuoteIdentifier(databaseName)}.sqlite_master m
                      JOIN pragma_index_list(m.name, $schema) i ON true
                      WHERE m.type = 'table'
                          AND m.name NOT LIKE 'sqlite\_%' ESCAPE '\'
                          AND m.sql NOT LIKE 'CREATE VIRTUAL%'
                      """;
                command.Parameters.AddWithValue("$schema", databaseName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dataTable.Rows.Add(
                            databaseName,
                            reader.GetString(3),
                            reader.GetString(0),
                            reader.GetInt64(1) != 0,
                            reader.IsDBNull(2) ? DBNull.Value : reader.GetString(2));
                    }
                }
            }

            return dataTable;
        }

        throw new ArgumentException(Resources.UnknownCollection(collectionName));
    }

    private List<string> GetDatabaseNames()
    {
        using var command = CreateCommand();
        command.CommandText = "PRAGMA database_list";
        using var reader = command.ExecuteReader();

        var databaseNames = new List<string>();
        while (reader.Read())
        {
            databaseNames.Add(reader.GetString(1));
        }

        return databaseNames;
    }

    private List<(string DatabaseName, string TableName, string TableType)> GetSchemaObjects()
    {
        var schemaObjects = new List<(string DatabaseName, string TableName, string TableType)>();

        if (IsTableListSupported(new Version(ServerVersion)))
        {
            using var command = CreateCommand();
            command.CommandText = "SELECT schema, name, type FROM pragma_table_list WHERE type IN ('table', 'view', 'virtual') AND name NOT LIKE 'sqlite\\_%' ESCAPE '\\'";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                schemaObjects.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
            }

            return schemaObjects;
        }

        foreach (var databaseName in GetDatabaseNames())
        {
            using var command = CreateCommand();
            command.CommandText = $"SELECT name, CASE WHEN type = 'table' AND sql LIKE 'CREATE VIRTUAL%' THEN 'virtual' ELSE type END FROM {QuoteIdentifier(databaseName)}.sqlite_master WHERE type IN ('table', 'view') AND name NOT LIKE 'sqlite\\_%' ESCAPE '\\'";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                schemaObjects.Add((databaseName, reader.GetString(0), reader.GetString(1)));
            }
        }

        return schemaObjects;
    }

    internal static bool IsTableListSupported(Version sqliteVersion)
        => sqliteVersion >= new Version(3, 37);

    internal static bool IsTableXInfoSupported(Version sqliteVersion)
        => sqliteVersion >= new Version(3, 26);

    internal static bool IsUnavailableSchemaObjectException(SqliteException exception)
        => exception.SqliteErrorCode == SQLITE_ERROR
            && (exception.Message?.Contains("no such table:", StringComparison.Ordinal) == true
                || exception.Message?.Contains("no such module:", StringComparison.Ordinal) == true
                || exception.Message?.Contains("no such column:", StringComparison.Ordinal) == true);

    internal static string GetNormalizedVersion(Version version)
        => $"{version.Major:00}.{version.Minor:000}.{version.Build:0000}.{(version.Revision < 0 ? 0 : version.Revision):0000}";

    private static string QuoteIdentifier(string identifier)
        => "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    internal static SupportedJoinOperators GetSupportedJoinOperators(Version sqliteVersion)
        => sqliteVersion >= new Version(3, 39)
            ? SupportedJoinOperators.Inner | SupportedJoinOperators.LeftOuter
                | SupportedJoinOperators.RightOuter | SupportedJoinOperators.FullOuter
            : SupportedJoinOperators.Inner | SupportedJoinOperators.LeftOuter;

    private void CreateFunctionCore<TState, TResult>(
        string name,
        int arity,
        TState state,
        Func<TState, SqliteValueReader, TResult>? function,
        bool isDeterministic)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        delegate_function_scalar? func = null;
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
                Handle,
                name,
                arity,
                flags,
                state,
                func);
            SqliteException.ThrowExceptionForRC(rc, Handle);
        }

        _functions ??= [with(FunctionsKeyComparer.Instance)];
        _functions[(name, arity)] = (flags, state, func);
    }

    private void CreateAggregateCore<TAccumulate, TResult>(
        string name,
        int arity,
        TAccumulate seed,
        Func<TAccumulate, SqliteValueReader, TAccumulate>? func,
        Func<TAccumulate, TResult>? resultSelector,
        bool isDeterministic)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        delegate_function_aggregate_step? func_step = null;
        if (func != null)
        {
            func_step = static (ctx, user_data, args) =>
            {
                var definition = (AggregateDefinition<TAccumulate, TResult>)user_data;
                ctx.state ??= new AggregateContext<TAccumulate>(definition.Seed);

                var context = (AggregateContext<TAccumulate>)ctx.state;
                if (context.Exception != null)
                {
                    return;
                }

                // TODO: Avoid allocation when niladic
                var reader = new SqliteParameterReader(definition.Name, args);

                try
                {
                    // NB: No need to set ctx.state since we just mutate the instance
                    context.Accumulate = definition.Func!(context.Accumulate, reader);
                }
                catch (Exception ex)
                {
                    context.Exception = ex;
                }
            };
        }

        delegate_function_aggregate_final? func_final = null;
        if (resultSelector != null)
        {
            func_final = static (ctx, user_data) =>
            {
                var definition = (AggregateDefinition<TAccumulate, TResult>)user_data;
                ctx.state ??= new AggregateContext<TAccumulate>(definition.Seed);

                var context = (AggregateContext<TAccumulate>)ctx.state;

                if (context.Exception == null)
                {
                    try
                    {
                        var result = definition.ResultSelector!(context.Accumulate);

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
        var state = new AggregateDefinition<TAccumulate, TResult>(name, seed, func, resultSelector);

        if (State == ConnectionState.Open)
        {
            var rc = sqlite3_create_function(
                Handle,
                name,
                arity,
                flags,
                state,
                func_step,
                func_final);
            SqliteException.ThrowExceptionForRC(rc, Handle);
        }

        _aggregates ??=
        [
            with(
                FunctionsKeyComparer.Instance)
        ];
        _aggregates[(name, arity)] = (flags, state, func_step, func_final);
    }

    private static Func<TState, SqliteValueReader, TResult>? IfNotNull<TState, TResult>(
        object? x,
        Func<TState, SqliteValueReader, TResult> value)
        => x != null ? value : null;

    private static object?[] GetValues(SqliteValueReader reader)
    {
        var values = new object?[reader.FieldCount];
        reader.GetValues(values);

        return values;
    }

    private sealed class AggregateDefinition<TAccumulate, TResult>(
        string name,
        TAccumulate seed,
        Func<TAccumulate, SqliteValueReader, TAccumulate>? func,
        Func<TAccumulate, TResult>? resultSelector)
    {
        public string Name { get; } = name;
        public TAccumulate Seed { get; } = seed;
        public Func<TAccumulate, SqliteValueReader, TAccumulate>? Func { get; } = func;
        public Func<TAccumulate, TResult>? ResultSelector { get; } = resultSelector;
    }

    private sealed class AggregateContext<T>(T seed)
    {
        public T Accumulate { get; set; } = seed;
        public Exception? Exception { get; set; }
    }

    private sealed class FunctionsKeyComparer : IEqualityComparer<(string name, int arity)>
    {
        public static readonly FunctionsKeyComparer Instance = new();

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
