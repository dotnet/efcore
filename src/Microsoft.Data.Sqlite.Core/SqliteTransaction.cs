// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite.Properties;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents a transaction made against a SQLite database.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/transactions">Transactions</seealso>
    public class SqliteTransaction : DbTransaction
    {
        private SqliteConnection? _connection;
        private bool _completed;

        internal SqliteTransaction(SqliteConnection connection, IsolationLevel isolationLevel, bool deferred)
        {
            if ((isolationLevel == IsolationLevel.ReadUncommitted
                    && ((connection.ConnectionOptions!.Cache != SqliteCacheMode.Shared) || !deferred))
                || isolationLevel == IsolationLevel.ReadCommitted
                || isolationLevel == IsolationLevel.RepeatableRead
                || isolationLevel == IsolationLevel.Unspecified)
            {
                isolationLevel = IsolationLevel.Serializable;
            }

            _connection = connection;
            IsolationLevel = isolationLevel;

            if (isolationLevel == IsolationLevel.ReadUncommitted)
            {
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 1;");
            }
            else if (isolationLevel != IsolationLevel.Serializable)
            {
                throw new ArgumentException(Resources.InvalidIsolationLevel(isolationLevel));
            }

            connection.ExecuteNonQuery(
                IsolationLevel == IsolationLevel.Serializable && !deferred
                    ? "BEGIN IMMEDIATE;"
                    : "BEGIN;");
            sqlite3_rollback_hook(connection.Handle, RollbackExternal, null);
        }

        /// <summary>
        ///     Gets the connection associated with the transaction.
        /// </summary>
        /// <value>The connection associated with the transaction.</value>
        public new virtual SqliteConnection? Connection
            => _connection;

        /// <summary>
        ///     Gets the connection associated with the transaction.
        /// </summary>
        /// <value>The connection associated with the transaction.</value>
        protected override DbConnection? DbConnection
            => Connection;

        internal bool ExternalRollback { get; private set; }

        /// <summary>
        ///     Gets the isolation level for the transaction.
        /// </summary>
        /// <value>The isolation level for the transaction.</value>
        public override IsolationLevel IsolationLevel { get; }

        /// <summary>
        ///     Applies the changes made in the transaction.
        /// </summary>
        public override void Commit()
        {
            if (ExternalRollback
                || _completed
                || _connection!.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.TransactionCompleted);
            }

            sqlite3_rollback_hook(_connection.Handle, null, null);
            _connection.ExecuteNonQuery("COMMIT;");
            Complete();
        }

        /// <summary>
        ///     Reverts the changes made in the transaction.
        /// </summary>
        public override void Rollback()
        {
            if (_completed || _connection!.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.TransactionCompleted);
            }

            RollbackInternal();
        }

#if NET5_0_OR_GREATER
        /// <inheritdoc />
        public override bool SupportsSavepoints => true;
#endif

        /// <summary>
        /// Creates a savepoint in the transaction. This allows all commands that are executed after the savepoint was
        /// established to be rolled back, restoring the transaction state to what it was at the time of the savepoint.
        /// </summary>
        /// <param name="savepointName">The name of the savepoint to be created.</param>
#if NET5_0_OR_GREATER
        public override void Save(string savepointName)
#else
        public virtual void Save(string savepointName)
#endif
        {
            if (savepointName is null)
            {
                throw new ArgumentNullException(nameof(savepointName));
            }

            if (_completed || _connection!.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.TransactionCompleted);
            }

            _connection.ExecuteNonQuery(
                new StringBuilder()
                    .Append("SAVEPOINT \"")
                    .Append(savepointName.Replace("\"", "\"\""))
                    .Append("\";")
                    .ToString());
        }

        /// <summary>
        /// Rolls back all commands that were executed after the specified savepoint was established.
        /// </summary>
        /// <param name="savepointName">The name of the savepoint to roll back to.</param>
#if NET5_0_OR_GREATER
        public override void Rollback(string savepointName)
#else
        public virtual void Rollback(string savepointName)
#endif
        {
            if (savepointName is null)
            {
                throw new ArgumentNullException(nameof(savepointName));
            }

            if (_completed || _connection!.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.TransactionCompleted);
            }

            _connection.ExecuteNonQuery(
                new StringBuilder()
                    .Append("ROLLBACK TO SAVEPOINT \"")
                    .Append(savepointName.Replace("\"", "\"\""))
                    .Append("\";")
                    .ToString());
        }

        /// <summary>
        /// Destroys a savepoint previously defined in the current transaction. This allows the system to
        /// reclaim some resources before the transaction ends.
        /// </summary>
        /// <param name="savepointName">The name of the savepoint to release.</param>
#if NET5_0_OR_GREATER
        public override void Release(string savepointName)
#else
        public virtual void Release(string savepointName)
#endif
        {
            if (savepointName is null)
            {
                throw new ArgumentNullException(nameof(savepointName));
            }

            if (_completed || _connection!.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.TransactionCompleted);
            }

            _connection.ExecuteNonQuery(
                new StringBuilder()
                    .Append("RELEASE SAVEPOINT \"")
                    .Append(savepointName.Replace("\"", "\"\""))
                    .Append("\";")
                    .ToString());
        }

        /// <summary>
        ///     Releases any resources used by the transaction and rolls it back.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing
                && !_completed
                && _connection!.State == ConnectionState.Open)
            {
                RollbackInternal();
            }
        }

        private void Complete()
        {
            if (IsolationLevel == IsolationLevel.ReadUncommitted)
            {
                try
                {
                    _connection!.ExecuteNonQuery("PRAGMA read_uncommitted = 0;");
                }
                catch
                {
                    // Ignore failure attempting to clean up.
                }
            }

            _connection!.Transaction = null;
            _connection = null;
            _completed = true;
        }

        private void RollbackInternal()
        {
            try
            {
                if (!ExternalRollback)
                {
                    sqlite3_rollback_hook(_connection!.Handle, null, null);
                    _connection.ExecuteNonQuery("ROLLBACK;");
                }
            }
            finally
            {
                Complete();
            }

        }

        private void RollbackExternal(object userData)
        {
            sqlite3_rollback_hook(_connection!.Handle, null, null);
            ExternalRollback = true;
        }
    }
}
