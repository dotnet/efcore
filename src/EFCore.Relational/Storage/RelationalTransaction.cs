// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         A transaction against the database.
    ///     </para>
    ///     <para>
    ///         Instances of this class are typically obtained from <see cref="DatabaseFacade.BeginTransaction" /> and it is not designed
    ///         to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class RelationalTransaction : IDbContextTransaction, IInfrastructure<DbTransaction>
    {
        private readonly IRelationalConnection _relationalConnection;
        private readonly DbTransaction _dbTransaction;
        private readonly IInterceptingLogger<LoggerCategory.Database.Transaction> _logger;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly bool _transactionOwned;

        private bool _connectionClosed;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTransaction" /> class.
        /// </summary>
        /// <param name="connection"> The connection to the database. </param>
        /// <param name="transaction"> The underlying <see cref="DbTransaction" />. </param>
        /// <param name="logger"> The logger to write to. </param>
        /// <param name="diagnosticSource"> The diagnostic source to write to. </param>
        /// <param name="transactionOwned">
        ///     A value indicating whether the transaction is owned by this class (i.e. if it can be disposed when this class is disposed).
        /// </param>
        public RelationalTransaction(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            [NotNull] IInterceptingLogger<LoggerCategory.Database.Transaction> logger,
            [NotNull] DiagnosticSource diagnosticSource,
            bool transactionOwned)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(transaction, nameof(transaction));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(diagnosticSource, nameof(diagnosticSource));

            if (connection.DbConnection != transaction.Connection)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAssociatedWithDifferentConnection);
            }

            _relationalConnection = connection;

            _dbTransaction = transaction;
            _logger = logger;
            _diagnosticSource = diagnosticSource;
            _transactionOwned = transactionOwned;

            _diagnosticSource.WriteTransactionStarted(
                _relationalConnection.DbConnection, 
                _relationalConnection.ConnectionId, 
                _dbTransaction);
        }

        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        public virtual void Commit()
        {
            _logger.LogDebug(
                RelationalEventId.CommittingTransaction,
                () => RelationalStrings.RelationalLoggerCommittingTransaction);

            var startTimestamp = Stopwatch.GetTimestamp();

            try
            {
                _dbTransaction.Commit();

                var currentTimestamp = Stopwatch.GetTimestamp();

                _diagnosticSource.WriteTransactionCommit(
                    _relationalConnection.DbConnection, 
                    _relationalConnection.ConnectionId,
                    _dbTransaction,
                    startTimestamp,
                    currentTimestamp);
            }
            catch (Exception e)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();

                _diagnosticSource.WriteTransactionError(
                    _relationalConnection.DbConnection, 
                    _relationalConnection.ConnectionId,
                    _dbTransaction, 
                    "Commit",
                    e,
                    startTimestamp,
                    currentTimestamp);
                throw;
            }

            ClearTransaction();
        }

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        public virtual void Rollback()
        {
            _logger.LogDebug(
                RelationalEventId.RollingbackTransaction,
                () => RelationalStrings.RelationalLoggerRollingbackTransaction);

            var startTimestamp = Stopwatch.GetTimestamp();

            try
            {
                _dbTransaction.Rollback();

                var currentTimestamp = Stopwatch.GetTimestamp();

                _diagnosticSource.WriteTransactionRollback(
                    _relationalConnection.DbConnection,
                    _relationalConnection.ConnectionId,
                    _dbTransaction,
                    startTimestamp,
                    currentTimestamp);
            }
            catch (Exception e)
            {
                var currentTimestamp = Stopwatch.GetTimestamp();

                _diagnosticSource.WriteTransactionError(
                    _relationalConnection.DbConnection,
                    _relationalConnection.ConnectionId,
                    _dbTransaction,
                    "Rollback",
                    e,
                    startTimestamp,
                    currentTimestamp);
                throw;
            }

            ClearTransaction();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_transactionOwned)
                {
                    _dbTransaction.Dispose();

                    _diagnosticSource.WriteTransactionDisposed(
                        _relationalConnection.DbConnection, 
                        _relationalConnection.ConnectionId, 
                        _dbTransaction);
                }

                ClearTransaction();
            }
        }

        private void ClearTransaction()
        {
            Debug.Assert((_relationalConnection.CurrentTransaction == null) || (_relationalConnection.CurrentTransaction == this));

            _relationalConnection.UseTransaction(null);

            if (!_connectionClosed)
            {
                _connectionClosed = true;

                _relationalConnection.Close();
            }
        }

        DbTransaction IInfrastructure<DbTransaction>.Instance => _dbTransaction;
    }
}
