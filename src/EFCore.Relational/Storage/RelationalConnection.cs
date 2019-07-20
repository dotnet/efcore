// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents a connection with a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public abstract class RelationalConnection : IRelationalConnection, ITransactionEnlistmentManager
    {
        private readonly string _connectionString;
        private readonly bool _connectionOwned;
        private int _openedCount;
        private bool _openedInternally;
        private int? _commandTimeout;
        private Transaction _ambientTransaction;
        private DbConnection _connection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalConnection" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Context = dependencies.CurrentContext.Context;

            Dependencies = dependencies;

            var relationalOptions = RelationalOptionsExtension.Extract(dependencies.ContextOptions);

            _commandTimeout = relationalOptions.CommandTimeout;

            if (relationalOptions.Connection != null)
            {
                if (!string.IsNullOrWhiteSpace(relationalOptions.ConnectionString))
                {
                    throw new InvalidOperationException(RelationalStrings.ConnectionAndConnectionString);
                }

                _connection = relationalOptions.Connection;
                _connectionOwned = false;
            }
            else if (!string.IsNullOrWhiteSpace(relationalOptions.ConnectionString))
            {
                _connectionString = dependencies.ConnectionStringResolver.ResolveConnectionString(relationalOptions.ConnectionString);
                _connectionOwned = true;
            }
            else
            {
                throw new InvalidOperationException(RelationalStrings.NoConnectionOrConnectionString);
            }
        }

        /// <summary>
        ///     The unique identifier for this connection.
        /// </summary>
        public virtual Guid ConnectionId { get; } = Guid.NewGuid();

        /// <summary>
        ///     The <see cref="DbContext"/> currently in use, or null if not known.
        /// </summary>
        public virtual DbContext Context { get; }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalConnectionDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a <see cref="DbConnection" /> to the database.
        /// </summary>
        /// <returns> The connection. </returns>
        protected abstract DbConnection CreateDbConnection();

        /// <summary>
        ///     Gets the connection string for the database.
        /// </summary>
        public virtual string ConnectionString => _connectionString ?? DbConnection.ConnectionString;

        /// <summary>
        ///     Gets the underlying <see cref="System.Data.Common.DbConnection" /> used to connect to the database.
        /// </summary>
        public virtual DbConnection DbConnection
            => _connection ??= CreateDbConnection();

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        public virtual IDbContextTransaction CurrentTransaction { get; [param: CanBeNull] protected set; }

        /// <summary>
        ///     The currently enlisted transaction.
        /// </summary>
        public virtual Transaction EnlistedTransaction
        {
            get
            {
                if (_enlistedTransaction != null)
                {
                    try
                    {
                        if (_enlistedTransaction.TransactionInformation.Status != TransactionStatus.Active)
                        {
                            _enlistedTransaction = null;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        _enlistedTransaction = null;
                    }
                }

                return _enlistedTransaction;
            }
            [param: CanBeNull] protected set => _enlistedTransaction = value;
        }

        /// <summary>
        ///     Specifies an existing <see cref="Transaction" /> to be used for database operations.
        /// </summary>
        /// <param name="transaction"> The transaction to be used. </param>
        public virtual void EnlistTransaction(Transaction transaction)
        {
            if (transaction != null)
            {
                Dependencies.TransactionLogger.ExplicitTransactionEnlisted(this, transaction);
            }

            DbConnection.EnlistTransaction(transaction);

            EnlistedTransaction = transaction;
        }

        /// <summary>
        ///     Indicates whether the store connection supports ambient transactions
        /// </summary>
        protected virtual bool SupportsAmbientTransactions => false;

        /// <summary>
        ///     Gets the timeout for executing a command against the database.
        /// </summary>
        public virtual int? CommandTimeout
        {
            get => _commandTimeout;
            set
            {
                if (value.HasValue
                    && value < 0)
                {
                    throw new ArgumentException(RelationalStrings.InvalidCommandTimeout);
                }

                _commandTimeout = value;
            }
        }

        /// <summary>
        ///     Begins a new transaction.
        /// </summary>
        /// <returns> The newly created transaction. </returns>
        [NotNull]
        // ReSharper disable once RedundantNameQualifier
        public virtual IDbContextTransaction BeginTransaction() => BeginTransaction(System.Data.IsolationLevel.Unspecified);

        /// <summary>
        ///     Asynchronously begins a new transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
        /// </returns>
        [NotNull]
        public virtual async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            // ReSharper disable once RedundantNameQualifier
            => await BeginTransactionAsync(System.Data.IsolationLevel.Unspecified, cancellationToken);

        /// <summary>
        ///     Begins a new transaction.
        /// </summary>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <returns> The newly created transaction. </returns>
        // ReSharper disable once RedundantNameQualifier
        public virtual IDbContextTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            Open();

            EnsureNoTransactions();

            var transactionId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            var interceptionResult = Dependencies.TransactionLogger.TransactionStarting(
                this,
                isolationLevel,
                transactionId,
                startTime);

            var dbTransaction = interceptionResult.HasResult
                ? interceptionResult.Result
                : DbConnection.BeginTransaction(isolationLevel);

            dbTransaction = Dependencies.TransactionLogger.TransactionStarted(
                this,
                dbTransaction,
                transactionId,
                startTime,
                stopwatch.Elapsed);

            return CreateRelationalTransaction(dbTransaction, transactionId, true);
        }

        /// <summary>
        ///     Asynchronously begins a new transaction.
        /// </summary>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
        /// </returns>
        public virtual async Task<IDbContextTransaction> BeginTransactionAsync(
            // ReSharper disable once RedundantNameQualifier
            System.Data.IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
        {
            await OpenAsync(cancellationToken);

            EnsureNoTransactions();

            var transactionId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            var interceptionResult = await Dependencies.TransactionLogger.TransactionStartingAsync(
                this,
                isolationLevel,
                transactionId,
                startTime,
                cancellationToken);

            var dbTransaction = interceptionResult.HasResult
                ? interceptionResult.Result
                : await DbConnection.BeginTransactionAsync(isolationLevel, cancellationToken);

            dbTransaction = await Dependencies.TransactionLogger.TransactionStartedAsync(
                this,
                dbTransaction,
                transactionId,
                startTime,
                stopwatch.Elapsed,
                cancellationToken);

            return CreateRelationalTransaction(dbTransaction, transactionId, true);
        }

        private void EnsureNoTransactions()
        {
            if (CurrentTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
            }

            if (Transaction.Current != null)
            {
                throw new InvalidOperationException(RelationalStrings.ConflictingAmbientTransaction);
            }

            if (EnlistedTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.ConflictingEnlistedTransaction);
            }
        }

        private IDbContextTransaction CreateRelationalTransaction(DbTransaction transaction, Guid transactionId, bool transactionOwned)
            => CurrentTransaction
                = Dependencies.RelationalTransactionFactory.Create(
                    this,
                    transaction,
                    transactionId,
                    Dependencies.TransactionLogger,
                    transactionOwned: transactionOwned);

        /// <summary>
        ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
        /// </summary>
        /// <param name="transaction"> The transaction to be used. </param>
        public virtual IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            if (ShouldUseTransaction(transaction))
            {
                Open();

                var transactionId = Guid.NewGuid();

                transaction = Dependencies.TransactionLogger.TransactionUsed(
                    this,
                    // ReSharper disable once AssignNullToNotNullAttribute
                    transaction,
                    transactionId,
                    DateTimeOffset.UtcNow);

                CurrentTransaction = CreateRelationalTransaction(transaction, transactionId, transactionOwned: false);
            }

            return CurrentTransaction;
        }

        /// <summary>
        ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
        /// </summary>
        /// <param name="transaction"> The transaction to be used. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> An instance of <see cref="IDbTransaction" /> that wraps the provided transaction. </returns>
        public virtual async Task<IDbContextTransaction> UseTransactionAsync(
            DbTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            if (ShouldUseTransaction(transaction))
            {
                await OpenAsync(cancellationToken);

                var transactionId = Guid.NewGuid();

                transaction = await Dependencies.TransactionLogger.TransactionUsedAsync(
                    this,
                    // ReSharper disable once AssignNullToNotNullAttribute
                    transaction,
                    transactionId,
                    DateTimeOffset.UtcNow,
                    cancellationToken);

                CurrentTransaction = CreateRelationalTransaction(transaction, transactionId, transactionOwned: false);
            }

            return CurrentTransaction;
        }

        private bool ShouldUseTransaction(DbTransaction transaction)
        {
            if (transaction == null)
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction = null;
                }

                return false;
            }

            EnsureNoTransactions();

            return true;
        }

        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        public virtual void CommitTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            CurrentTransaction.Commit();
        }

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        public virtual void RollbackTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            CurrentTransaction.Rollback();
        }

        /// <summary>
        ///     Opens the connection to the database.
        /// </summary>
        /// <param name="errorsExpected">
        ///     Indicates if the connection errors are expected and should be logged as debug message.
        /// </param>
        /// <returns> True if the underlying connection was actually opened; false otherwise. </returns>
        public virtual bool Open(bool errorsExpected = false)
        {
            if (DbConnection.State == ConnectionState.Broken)
            {
                DbConnection.Close();
            }

            var wasOpened = false;
            if (DbConnection.State != ConnectionState.Open)
            {
                CurrentTransaction?.Dispose();
                ClearTransactions(clearAmbient: false);
                OpenDbConnection(errorsExpected);
                wasOpened = true;
            }

            _openedCount++;

            HandleAmbientTransactions();

            return wasOpened;
        }

        /// <summary>
        ///     Asynchronously opens the connection to the database.
        /// </summary>
        /// <param name="errorsExpected"> Indicate if the connection errors are expected and should be logged as debug message. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, with a value of true if the connection
        ///     was actually opened.
        /// </returns>
        public virtual async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
        {
            if (DbConnection.State == ConnectionState.Broken)
            {
                await DbConnection.CloseAsync();
            }

            var wasOpened = false;
            if (DbConnection.State != ConnectionState.Open)
            {
                if (CurrentTransaction != null)
                {
                    await CurrentTransaction.DisposeAsync();
                }

                ClearTransactions(clearAmbient: false);
                await OpenDbConnectionAsync(errorsExpected, cancellationToken);
                wasOpened = true;
            }

            _openedCount++;

            HandleAmbientTransactions();

            return wasOpened;
        }

        private void ClearTransactions(bool clearAmbient)
        {
            CurrentTransaction = null;
            EnlistedTransaction = null;
            if (clearAmbient
                && _ambientTransaction != null)
            {
                _ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
                _ambientTransaction = null;
            }

            if (_openedCount < 0)
            {
                _openedCount = 0;
            }
        }

        private void OpenDbConnection(bool errorsExpected)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            var interceptionResult = Dependencies.ConnectionLogger.ConnectionOpening(this, startTime);

            try
            {
                if (!interceptionResult.IsSuppressed)
                {
                    DbConnection.Open();
                }

                Dependencies.ConnectionLogger.ConnectionOpened(this, startTime, stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                Dependencies.ConnectionLogger.ConnectionError(this, e, startTime, stopwatch.Elapsed, errorsExpected);

                throw;
            }

            if (_openedCount == 0)
            {
                _openedInternally = true;
            }
        }

        private async Task OpenDbConnectionAsync(bool errorsExpected, CancellationToken cancellationToken)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            var interceptionResult
                = await Dependencies.ConnectionLogger.ConnectionOpeningAsync(this, startTime, cancellationToken);

            try
            {
                if (!interceptionResult.IsSuppressed)
                {
                    await DbConnection.OpenAsync(cancellationToken);
                }

                await Dependencies.ConnectionLogger.ConnectionOpenedAsync(this, startTime, stopwatch.Elapsed, cancellationToken);
            }
            catch (Exception e)
            {
                await Dependencies.ConnectionLogger.ConnectionErrorAsync(
                    this,
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    errorsExpected,
                    cancellationToken);

                throw;
            }

            if (_openedCount == 0)
            {
                _openedInternally = true;
            }
        }

        private void HandleAmbientTransactions()
        {
            var current = Transaction.Current;
            if (current != null
                && !SupportsAmbientTransactions)
            {
                Dependencies.TransactionLogger.AmbientTransactionWarning(this, DateTimeOffset.UtcNow);
            }

            if (Equals(current, _ambientTransaction))
            {
                return;
            }

            if (_ambientTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.PendingAmbientTransaction);
            }

            if (current != null)
            {
                Dependencies.TransactionLogger.AmbientTransactionEnlisted(this, current);
            }

            DbConnection.EnlistTransaction(current);

            var ambientTransaction = _ambientTransaction;
            if (ambientTransaction != null)
            {
                ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
            }

            if (current != null)
            {
                current.TransactionCompleted += HandleTransactionCompleted;
            }

            _ambientTransaction = current;
        }

        private void HandleTransactionCompleted(object sender, TransactionEventArgs e)
        {
            // This could be invoked on a different thread at arbitrary time after the transaction completes
            var ambientTransaction = _ambientTransaction;
            if (ambientTransaction != null)
            {
                ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
                _ambientTransaction = null;
            }
        }

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns> True if the underlying connection was actually closed; false otherwise. </returns>
        public virtual bool Close()
        {
            var wasClosed = false;

            if (ShouldClose())
            {
                CurrentTransaction?.Dispose();
                ClearTransactions(clearAmbient: false);

                if (DbConnection.State != ConnectionState.Closed)
                {
                    var startTime = DateTimeOffset.UtcNow;
                    var stopwatch = Stopwatch.StartNew();

                    var interceptionResult = Dependencies.ConnectionLogger.ConnectionClosing(this, startTime);

                    try
                    {
                        if (!interceptionResult.IsSuppressed)
                        {
                            DbConnection.Close();
                        }

                        wasClosed = true;

                        Dependencies.ConnectionLogger.ConnectionClosed(this, startTime, stopwatch.Elapsed);
                    }
                    catch (Exception e)
                    {
                        Dependencies.ConnectionLogger.ConnectionError(this, e, startTime, stopwatch.Elapsed, false);

                        throw;
                    }
                }

                _openedInternally = false;
            }

            return wasClosed;
        }

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation, with a value of true if the connection
        ///     was actually closed.
        /// </returns>
        public virtual async Task<bool> CloseAsync()
        {
            var wasClosed = false;

            if (ShouldClose())
            {
                if (CurrentTransaction != null)
                {
                    await CurrentTransaction.DisposeAsync();
                }
                ClearTransactions(clearAmbient: false);

                if (DbConnection.State != ConnectionState.Closed)
                {
                    var startTime = DateTimeOffset.UtcNow;
                    var stopwatch = Stopwatch.StartNew();

                    var interceptionResult = await Dependencies.ConnectionLogger.ConnectionClosingAsync(
                        this,
                        startTime);

                    try
                    {
                        if (!interceptionResult.IsSuppressed)
                        {
                            await DbConnection.CloseAsync();
                        }

                        wasClosed = true;

                        await Dependencies.ConnectionLogger.ConnectionClosedAsync(
                            this,
                            startTime,
                            stopwatch.Elapsed);
                    }
                    catch (Exception e)
                    {
                        await Dependencies.ConnectionLogger.ConnectionErrorAsync(
                            this,
                            e,
                            startTime,
                            stopwatch.Elapsed,
                            false);

                        throw;
                    }
                }

                _openedInternally = false;
            }

            return wasClosed;
        }

        private bool ShouldClose()
            => (_openedCount == 0
                || _openedCount > 0
                && --_openedCount == 0)
               && _openedInternally;

        /// <summary>
        ///     Gets a value indicating whether the multiple active result sets feature is enabled.
        /// </summary>
        public virtual bool IsMultipleActiveResultSetsEnabled => false;

        void IResettableService.ResetState() => Dispose();

        ValueTask IResettableService.ResetStateAsync() => DisposeAsync();

        /// <summary>
        ///     Gets a semaphore used to serialize access to this connection.
        /// </summary>
        /// <value>
        ///     The semaphore used to serialize access to this connection.
        /// </value>
        public virtual SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

        private Transaction _enlistedTransaction;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            CurrentTransaction?.Dispose();
            ClearTransactions(clearAmbient: true);

            if (_connectionOwned
                && _connection != null)
            {
                DbConnection.Dispose();
                _connection = null;
                _openedCount = 0;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual async ValueTask DisposeAsync()
        {
            if (CurrentTransaction != null)
            {
                await CurrentTransaction.DisposeAsync();
            }
            ClearTransactions(clearAmbient: true);

            if (_connectionOwned
                && _connection != null)
            {
                await DbConnection.DisposeAsync();
                _connection = null;
                _openedCount = 0;
            }
        }
    }
}
