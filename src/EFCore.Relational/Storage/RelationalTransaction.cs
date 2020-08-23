// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        private readonly DbTransaction _dbTransaction;
        private readonly bool _transactionOwned;

        private bool _connectionClosed;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTransaction" /> class.
        /// </summary>
        /// <param name="connection"> The connection to the database. </param>
        /// <param name="transaction"> The underlying <see cref="DbTransaction" />. </param>
        /// <param name="transactionId"> The correlation ID for the transaction. </param>
        /// <param name="logger"> The logger to write to. </param>
        /// <param name="transactionOwned">
        ///     A value indicating whether the transaction is owned by this class (i.e. if it can be disposed when this class is disposed).
        /// </param>
        public RelationalTransaction(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(transaction, nameof(transaction));
            Check.NotNull(logger, nameof(logger));

            if (connection.DbConnection != transaction.Connection)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAssociatedWithDifferentConnection);
            }

            Connection = connection;
            TransactionId = transactionId;

            _dbTransaction = transaction;
            Logger = logger;
            _transactionOwned = transactionOwned;
        }

        /// <summary>
        ///     The connection.
        /// </summary>
        protected virtual IRelationalConnection Connection { get; }

        /// <summary>
        ///     The logger.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> Logger { get; }

        /// <inheritdoc />
        public virtual Guid TransactionId { get; }

        /// <inheritdoc />
        public virtual void Commit()
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = Logger.TransactionCommitting(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);

                if (!interceptionResult.IsSuppressed)
                {
                    _dbTransaction.Commit();
                }

                Logger.TransactionCommitted(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                Logger.TransactionError(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "Commit",
                    e,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }

            ClearTransaction();
        }

        /// <inheritdoc />
        public virtual void Rollback()
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = Logger.TransactionRollingBack(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);

                if (!interceptionResult.IsSuppressed)
                {
                    _dbTransaction.Rollback();
                }

                Logger.TransactionRolledBack(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                Logger.TransactionError(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "Rollback",
                    e,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }

            ClearTransaction();
        }

        /// <inheritdoc />
        public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = await Logger.TransactionCommittingAsync(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        startTime,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (!interceptionResult.IsSuppressed)
                {
                    await _dbTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }

                await Logger.TransactionCommittedAsync(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await Logger.TransactionErrorAsync(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        "Commit",
                        e,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken)
                    .ConfigureAwait(false);

                throw;
            }

            await ClearTransactionAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = await Logger.TransactionRollingBackAsync(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        startTime,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (!interceptionResult.IsSuppressed)
                {
                    await _dbTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }

                await Logger.TransactionRolledBackAsync(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await Logger.TransactionErrorAsync(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        "Rollback",
                        e,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken)
                    .ConfigureAwait(false);

                throw;
            }

            await ClearTransactionAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual void CreateSavepoint(string name)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = Logger.CreatingTransactionSavepoint(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);

                if (!interceptionResult.IsSuppressed)
                {
                    using var command = Connection.DbConnection.CreateCommand();
                    command.Transaction = _dbTransaction;
                    command.CommandText = GetCreateSavepointSql(name);
                    command.ExecuteNonQuery();
                }

                Logger.CreatedTransactionSavepoint(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);
            }
            catch (Exception e)
            {
                Logger.TransactionError(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "CreateSavepoint",
                    e,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = await Logger.CreatingTransactionSavepointAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken).ConfigureAwait(false);

                if (!interceptionResult.IsSuppressed)
                {
                    using var command = Connection.DbConnection.CreateCommand();
                    command.Transaction = _dbTransaction;
                    command.CommandText = GetCreateSavepointSql(name);
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await Logger.CreatedTransactionSavepointAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await Logger.TransactionErrorAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "CreateSavepoint",
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    cancellationToken).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        ///     When implemented in a provider supporting transaction savepoints, this method should return an
        ///     SQL statement which creates a savepoint with the given name.
        /// </summary>
        /// <param name="name"> The name of the savepoint to be created. </param>
        /// <returns> An SQL string to create the savepoint. </returns>
        protected virtual string GetCreateSavepointSql([NotNull] string name)
            => "SAVEPOINT " + name;

        /// <inheritdoc />
        public virtual void RollbackToSavepoint(string name)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = Logger.RollingBackToTransactionSavepoint(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);

                if (!interceptionResult.IsSuppressed)
                {
                    using var command = Connection.DbConnection.CreateCommand();
                    command.Transaction = _dbTransaction;
                    command.CommandText = GetRollbackToSavepointSql(name);
                    command.ExecuteNonQuery();
                }

                Logger.RolledBackToTransactionSavepoint(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);
            }
            catch (Exception e)
            {
                Logger.TransactionError(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "RollbackToSavepoint",
                    e,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = await Logger.RollingBackToTransactionSavepointAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken).ConfigureAwait(false);

                if (!interceptionResult.IsSuppressed)
                {
                    using var command = Connection.DbConnection.CreateCommand();
                    command.Transaction = _dbTransaction;
                    command.CommandText = GetRollbackToSavepointSql(name);
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await Logger.RolledBackToTransactionSavepointAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await Logger.TransactionErrorAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "RollbackToSavepoint",
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    cancellationToken).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        ///     When implemented in a provider supporting transaction savepoints, this method should return an
        ///     SQL statement which rolls back a savepoint with the given name.
        /// </summary>
        /// <param name="name"> The name of the savepoint to be created. </param>
        /// <returns> An SQL string to create the savepoint. </returns>
        protected virtual string GetRollbackToSavepointSql([NotNull] string name)
            => "ROLLBACK TO " + name;

        /// <inheritdoc />
        public virtual void ReleaseSavepoint(string name)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = Logger.ReleasingTransactionSavepoint(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);

                if (!interceptionResult.IsSuppressed)
                {
                    using var command = Connection.DbConnection.CreateCommand();
                    command.Transaction = _dbTransaction;
                    command.CommandText = GetReleaseSavepointSql(name);
                    command.ExecuteNonQuery();
                }

                Logger.ReleasedTransactionSavepoint(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime);
            }
            catch (Exception e)
            {
                Logger.TransactionError(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "ReleaseSavepoint",
                    e,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = await Logger.ReleasingTransactionSavepointAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken).ConfigureAwait(false);

                if (!interceptionResult.IsSuppressed)
                {
                    using var command = Connection.DbConnection.CreateCommand();
                    command.Transaction = _dbTransaction;
                    command.CommandText = GetReleaseSavepointSql(name);
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await Logger.ReleasedTransactionSavepointAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await Logger.TransactionErrorAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "ReleaseSavepoint",
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    cancellationToken).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        ///     <para>
        ///         When implemented in a provider supporting transaction savepoints, this method should return an
        ///         SQL statement which releases a savepoint with the given name.
        ///     </para>
        ///     <para>
        ///         If savepoint release isn't supported, <see cref="ReleaseSavepoint " /> and <see cref="ReleaseSavepointAsync " /> should
        ///         be overridden to do nothing.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the savepoint to be created. </param>
        /// <returns> An SQL string to create the savepoint. </returns>
        protected virtual string GetReleaseSavepointSql([NotNull] string name)
            => "RELEASE SAVEPOINT " + name;

        /// <inheritdoc />
        public virtual bool SupportsSavepoints
            => true;

        /// <inheritdoc />
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_transactionOwned)
                {
                    _dbTransaction.Dispose();

                    Logger.TransactionDisposed(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        DateTimeOffset.UtcNow);
                }

                ClearTransaction();
            }
        }

        /// <inheritdoc />
        public virtual async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_transactionOwned)
                {
                    await _dbTransaction.DisposeAsync().ConfigureAwait(false);

                    Logger.TransactionDisposed(
                        Connection,
                        _dbTransaction,
                        TransactionId,
                        DateTimeOffset.UtcNow);
                }

                await ClearTransactionAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Remove the underlying transaction from the connection
        /// </summary>
        protected virtual void ClearTransaction()
        {
            Check.DebugAssert(
                Connection.CurrentTransaction == null || Connection.CurrentTransaction == this,
                "Connection.CurrentTransaction is unexpected instance");

            Connection.UseTransaction(null);

            if (!_connectionClosed)
            {
                _connectionClosed = true;

                Connection.Close();
            }
        }

        /// <summary>
        ///     Remove the underlying transaction from the connection
        /// </summary>
        protected virtual async Task ClearTransactionAsync(CancellationToken cancellationToken = default)
        {
            Check.DebugAssert(
                Connection.CurrentTransaction == null || Connection.CurrentTransaction == this,
                "Connection.CurrentTransaction is unexpected instance");

            await Connection.UseTransactionAsync(null, cancellationToken).ConfigureAwait(false);

            if (!_connectionClosed)
            {
                _connectionClosed = true;

                await Connection.CloseAsync().ConfigureAwait(false);
            }
        }

        DbTransaction IInfrastructure<DbTransaction>.Instance
            => _dbTransaction;
    }
}
