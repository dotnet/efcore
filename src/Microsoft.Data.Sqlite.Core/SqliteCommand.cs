// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents a SQL statement to be executed against a SQLite database.
    /// </summary>
    public class SqliteCommand : DbCommand
    {
        private readonly Lazy<SqliteParameterCollection> _parameters = new Lazy<SqliteParameterCollection>(
            () => new SqliteParameterCollection());

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteCommand" /> class.
        /// </summary>
        public SqliteCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteCommand" /> class.
        /// </summary>
        /// <param name="commandText">The SQL to execute against the database.</param>
        public SqliteCommand(string commandText)
        {
            CommandText = commandText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteCommand" /> class.
        /// </summary>
        /// <param name="commandText">The SQL to execute against the database.</param>
        /// <param name="connection">The connection used by the command.</param>
        public SqliteCommand(string commandText, SqliteConnection connection)
            : this(commandText)
        {
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteCommand" /> class.
        /// </summary>
        /// <param name="commandText">The SQL to execute against the database.</param>
        /// <param name="connection">The connection used by the command.</param>
        /// <param name="transaction">The transaction within which the command executes.</param>
        public SqliteCommand(string commandText, SqliteConnection connection, SqliteTransaction transaction)
            : this(commandText, connection)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// Gets or sets a value indicating how <see cref="CommandText" /> is interpreted. Only
        /// <see cref="CommandType.Text" /> is supported.
        /// </summary>
        /// <value>A value indicating how <see cref="CommandText" /> is interpreted.</value>
        public override CommandType CommandType
        {
            get => CommandType.Text;
            set
            {
                if (value != CommandType.Text)
                {
                    throw new ArgumentException(Resources.InvalidCommandType(value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the SQL to execute against the database.
        /// </summary>
        /// <value>The SQL to execute against the database.</value>
        public override string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the connection used by the command.
        /// </summary>
        /// <value>The connection used by the command.</value>
        public new virtual SqliteConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets the connection used by the command. Must be a <see cref="SqliteConnection" />.
        /// </summary>
        /// <value>The connection used by the command.</value>
        protected override DbConnection DbConnection
        {
            get => Connection;
            set => Connection = (SqliteConnection)value;
        }

        /// <summary>
        /// Gets or sets the transaction within which the command executes.
        /// </summary>
        /// <value>The transaction within which the command executes.</value>
        public new virtual SqliteTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the transaction within which the command executes. Must be a <see cref="SqliteTransaction" />.
        /// </summary>
        /// <value>The transaction within which the command executes.</value>
        protected override DbTransaction DbTransaction
        {
            get => Transaction;
            set => Transaction = (SqliteTransaction)value;
        }

        /// <summary>
        /// Gets the collection of parameters used by the command.
        /// </summary>
        /// <value>The collection of parameters used by the command.</value>
        public new virtual SqliteParameterCollection Parameters
            => _parameters.Value;

        /// <summary>
        /// Gets the collection of parameters used by the command.
        /// </summary>
        /// <value>The collection of parameters used by the command.</value>
        protected override DbParameterCollection DbParameterCollection
            => Parameters;

        /// <summary>
        /// Gets or sets the number of seconds to wait before terminating the attempt to execute the command. Defaults to 30.
        /// </summary>
        /// <value>The number of seconds to wait before terminating the attempt to execute the command.</value>
        /// <remarks>
        /// The timeout is used when the command is waiting to obtain a lock on the table.
        /// </remarks>
        public override int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating whether the command should be visible in an interface control.
        /// </summary>
        /// <value>A value indicating whether the command should be visible in an interface control.</value>
        public override bool DesignTimeVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how the results are applied to the row being updated.
        /// </summary>
        /// <value>A value indicating how the results are applied to the row being updated.</value>
        public override UpdateRowSource UpdatedRowSource { get; set; }

        /// <summary>
        /// Creates a new parameter.
        /// </summary>
        /// <returns>The new parameter.</returns>
        public new virtual SqliteParameter CreateParameter()
            => new SqliteParameter();

        /// <summary>
        /// Creates a new parameter.
        /// </summary>
        /// <returns>The new parameter.</returns>
        protected override DbParameter CreateDbParameter()
            => CreateParameter();

        /// <summary>
        /// Creates a prepared version of the command on the database. This has no effect.
        /// </summary>
        public override void Prepare()
        {
        }

        /// <summary>
        /// Executes the <see cref="CommandText" /> against the database and returns a data reader.
        /// </summary>
        /// <returns>The data reader.</returns>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        public new virtual SqliteDataReader ExecuteReader()
            => ExecuteReader(CommandBehavior.Default);

        /// <summary>
        /// Executes the <see cref="CommandText" /> against the database and returns a data reader.
        /// </summary>
        /// <param name="behavior">
        /// A description of the results of the query and its effect on the database.
        /// <para>Only <see cref="CommandBehavior.Default" />, <see cref="CommandBehavior.SequentialAccess" />,
        /// <see cref="CommandBehavior.SingleResult" />, <see cref="CommandBehavior.SingleRow" />, and
        /// <see cref="CommandBehavior.CloseConnection" /> are supported.</para>
        /// </param>
        /// <returns>The data reader.</returns>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        public new virtual SqliteDataReader ExecuteReader(CommandBehavior behavior)
        {
            if ((behavior & ~(CommandBehavior.Default | CommandBehavior.SequentialAccess | CommandBehavior.SingleResult
                | CommandBehavior.SingleRow | CommandBehavior.CloseConnection)) != 0)
            {
                throw new ArgumentException(Resources.InvalidCommandBehavior(behavior));
            }

            if (Connection == null
                || Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.CallRequiresOpenConnection(nameof(ExecuteReader)));
            }

            if (string.IsNullOrEmpty(CommandText))
            {
                throw new InvalidOperationException(Resources.CallRequiresSetCommandText(nameof(ExecuteReader)));
            }

            if (Transaction != Connection.Transaction)
            {
                throw new InvalidOperationException(
                    Transaction == null
                        ? Resources.TransactionRequired
                        : Resources.TransactionConnectionMismatch);
            }

            // This is not a guarantee. SQLITE_BUSY can still be thrown before the command timeout.
            // This sets a timeout handler but this can be cleared by concurrent commands.
            raw.sqlite3_busy_timeout(Connection.Handle, CommandTimeout * 1000);

            var hasChanges = false;
            var changes = 0;
            var stmts = new Queue<Tuple<sqlite3_stmt, bool>>();
            var tail = CommandText;

            do
            {
                var rc = raw.sqlite3_prepare_v2(
                        Connection.Handle,
                        tail,
                        out var stmt,
                        out tail);
                SqliteException.ThrowExceptionForRC(rc, Connection.Handle);

                // Statement was empty, white space, or a comment
                if (stmt.ptr == IntPtr.Zero)
                {
                    if (!string.IsNullOrEmpty(tail))
                    {
                        continue;
                    }

                    break;
                }

                var boundParams = 0;

                if (_parameters.IsValueCreated)
                {
                    boundParams = _parameters.Value.Bind(stmt);
                }

                var expectedParams = raw.sqlite3_bind_parameter_count(stmt);
                if (expectedParams != boundParams)
                {
                    var unboundParams = new List<string>();
                    for (var i = 1; i <= expectedParams; i++)
                    {
                        var name = raw.sqlite3_bind_parameter_name(stmt, i);

                        if (_parameters.IsValueCreated
                            ||
                            !_parameters.Value.Cast<SqliteParameter>().Any(p => p.ParameterName == name))
                        {
                            unboundParams.Add(name);
                        }
                    }

                    throw new InvalidOperationException(Resources.MissingParameters(string.Join(", ", unboundParams)));
                }

                try
                {
                    var timer = Stopwatch.StartNew();
                    while (raw.SQLITE_LOCKED == (rc = raw.sqlite3_step(stmt)) || rc == raw.SQLITE_BUSY)
                    {
                        if (timer.ElapsedMilliseconds >= CommandTimeout * 1000)
                        {
                            break;
                        }

                        raw.sqlite3_reset(stmt);

#if NET451
                        // TODO: Consider having an async path that uses Task.Delay()
                        Thread.Sleep(150);
#endif
                    }

                    SqliteException.ThrowExceptionForRC(rc, Connection.Handle);
                }
                catch
                {
                    stmt.Dispose();
                    throw;
                }

                // NB: This is only a heuristic to separate SELECT statements from INSERT/UPDATE/DELETE statements. It
                //     will result in unexpected corner cases, but it's the best we can do without re-parsing SQL
                if (raw.sqlite3_stmt_readonly(stmt) != 0)
                {
                    stmts.Enqueue(Tuple.Create(stmt, rc != raw.SQLITE_DONE));
                }
                else
                {
                    hasChanges = true;
                    changes += raw.sqlite3_changes(Connection.Handle);
                    stmt.Dispose();
                }
            }
            while (!string.IsNullOrEmpty(tail));

            var closeConnection = (behavior & CommandBehavior.CloseConnection) != 0;

            return new SqliteDataReader(Connection, stmts, hasChanges ? changes : -1, closeConnection);
        }

        /// <summary>
        /// Executes the <see cref="CommandText" /> against the database and returns a data reader.
        /// </summary>
        /// <param name="behavior">A description of query's results and its effect on the database.</param>
        /// <returns>The data reader.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => ExecuteReader(behavior);

        /// <summary>
        /// Executes the <see cref="CommandText" /> asynchronously against the database and returns a data reader.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// SQLite does not support asynchronous execution. Use write-ahead logging instead.
        /// </remarks>
        /// <seealso href="http://sqlite.org/wal.html">Write-Ahead Logging</seealso>
        public new virtual Task<SqliteDataReader> ExecuteReaderAsync()
            => ExecuteReaderAsync(CommandBehavior.Default, CancellationToken.None);

        /// <summary>
        /// Executes the <see cref="CommandText" /> asynchronously against the database and returns a data reader.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// SQLite does not support asynchronous execution. Use write-ahead logging instead.
        /// </remarks>
        /// <seealso href="http://sqlite.org/wal.html">Write-Ahead Logging</seealso>
        public new virtual Task<SqliteDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
            => ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);

        /// <summary>
        /// Executes the <see cref="CommandText" /> asynchronously against the database and returns a data reader.
        /// </summary>
        /// <param name="behavior">A description of query's results and its effect on the database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// SQLite does not support asynchronous execution. Use write-ahead logging instead.
        /// </remarks>
        /// <seealso href="http://sqlite.org/wal.html">Write-Ahead Logging</seealso>
        public new virtual Task<SqliteDataReader> ExecuteReaderAsync(CommandBehavior behavior)
            => ExecuteReaderAsync(behavior, CancellationToken.None);

        /// <summary>
        /// Executes the <see cref="CommandText" /> asynchronously against the database and returns a data reader.
        /// </summary>
        /// <param name="behavior">A description of query's results and its effect on the database.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// SQLite does not support asynchronous execution. Use write-ahead logging instead.
        /// </remarks>
        /// <seealso href="http://sqlite.org/wal.html">Write-Ahead Logging</seealso>
        public new virtual Task<SqliteDataReader> ExecuteReaderAsync(
            CommandBehavior behavior,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(ExecuteReader(behavior));
        }

        /// <summary>
        /// Executes the <see cref="CommandText" /> asynchronously against the database and returns a data reader.
        /// </summary>
        /// <param name="behavior">A description of query's results and its effect on the database.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(
            CommandBehavior behavior,
            CancellationToken cancellationToken)
            => await ExecuteReaderAsync(behavior, cancellationToken);

        /// <summary>
        /// Executes the <see cref="CommandText" /> against the database.
        /// </summary>
        /// <returns>The number of rows inserted, updated, or deleted. -1 for SELECT statements.</returns>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        public override int ExecuteNonQuery()
        {
            if (Connection == null
                || Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.CallRequiresOpenConnection(nameof(ExecuteNonQuery)));
            }
            if (CommandText == null)
            {
                throw new InvalidOperationException(Resources.CallRequiresSetCommandText(nameof(ExecuteNonQuery)));
            }

            var reader = ExecuteReader();
            reader.Dispose();

            return reader.RecordsAffected;
        }

        /// <summary>
        /// Executes the <see cref="CommandText" /> against the database and returns the result.
        /// </summary>
        /// <returns>The first column of the first row of the results, or null if no results.</returns>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        public override object ExecuteScalar()
        {
            if (Connection == null
                || Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.CallRequiresOpenConnection(nameof(ExecuteScalar)));
            }
            if (CommandText == null)
            {
                throw new InvalidOperationException(Resources.CallRequiresSetCommandText(nameof(ExecuteScalar)));
            }

            using (var reader = ExecuteReader())
            {
                return reader.Read()
                    ? reader.GetValue(0)
                    : null;
            }
        }

        /// <summary>
        /// Attempts to cancel the execution of the command. Does nothing.
        /// </summary>
        public override void Cancel()
        {
        }
    }
}
