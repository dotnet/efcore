// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
    /// </summary>
    public abstract class RelationalConnection : IRelationalConnection, ITransactionEnlistmentManager
    {
        private readonly string _connectionString;
        private readonly LazyRef<DbConnection> _connection;
        private readonly bool _connectionOwned;
        private int _openedCount;
        private bool _openedInternally;
        private int? _commandTimeout;
        private Transaction _ambientTransaction;
        private SemaphoreSlim _semaphore { get; } = new SemaphoreSlim(1);

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalConnection" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            var relationalOptions = RelationalOptionsExtension.Extract(dependencies.ContextOptions);

            _commandTimeout = relationalOptions.CommandTimeout;

            if (relationalOptions.Connection != null)
            {
                if (!string.IsNullOrWhiteSpace(relationalOptions.ConnectionString))
                {
                    throw new InvalidOperationException(RelationalStrings.ConnectionAndConnectionString);
                }

                _connection = new LazyRef<DbConnection>(() => relationalOptions.Connection);
                _connectionOwned = false;
            }
            else if (!string.IsNullOrWhiteSpace(relationalOptions.ConnectionString))
            {
                _connectionString = dependencies.ConnectionStringResolver.ResolveConnectionString(relationalOptions.ConnectionString);
                _connection = new LazyRef<DbConnection>(CreateDbConnection);
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
        public virtual DbConnection DbConnection => _connection.Value;

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
        [NotNull]
        // ReSharper disable once RedundantNameQualifier
        public virtual IDbContextTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            Open();

            EnsureNoTransactions();

            return BeginTransactionWithNoPreconditions(isolationLevel);
        }

        /// <summary>
        ///     Asynchronously begins a new transaction.
        /// </summary>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
        /// </returns>
        [NotNull]
        public virtual async Task<IDbContextTransaction> BeginTransactionAsync(
            // ReSharper disable once RedundantNameQualifier
            System.Data.IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
        {
            await OpenAsync(cancellationToken);

            EnsureNoTransactions();

            return BeginTransactionWithNoPreconditions(isolationLevel);
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

        // ReSharper disable once RedundantNameQualifier
        private IDbContextTransaction BeginTransactionWithNoPreconditions(System.Data.IsolationLevel isolationLevel)
        {
            var dbTransaction = DbConnection.BeginTransaction(isolationLevel);

            CurrentTransaction
                = Dependencies.RelationalTransactionFactory.Create(
                    this,
                    dbTransaction,
                    Dependencies.TransactionLogger,
                    transactionOwned: true);

            Dependencies.TransactionLogger.TransactionStarted(
                this,
                dbTransaction,
                CurrentTransaction.TransactionId,
                DateTimeOffset.UtcNow);

            return CurrentTransaction;
        }

        /// <summary>
        ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
        /// </summary>
        /// <param name="transaction"> The transaction to be used. </param>
        public virtual IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            if (transaction == null)
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction = null;
                }
            }
            else
            {
                EnsureNoTransactions();

                Open();

                CurrentTransaction = Dependencies.RelationalTransactionFactory.Create(
                    this,
                    transaction,
                    Dependencies.TransactionLogger,
                    transactionOwned: false);

                Dependencies.TransactionLogger.TransactionUsed(
                    this,
                    transaction,
                    CurrentTransaction.TransactionId,
                    DateTimeOffset.UtcNow);
            }

            return CurrentTransaction;
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
                OpenDbConnection(errorsExpected);
                wasOpened = true;
                ClearTransactions();
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
                DbConnection.Close();
            }

            var wasOpened = false;

            if (DbConnection.State != ConnectionState.Open)
            {
                await OpenDbConnectionAsync(errorsExpected, cancellationToken);
                wasOpened = true;
                ClearTransactions();
            }

            _openedCount++;

            HandleAmbientTransactions();

            return wasOpened;
        }

        private void ClearTransactions()
        {
            var previousOpenedCount = _openedCount;
            _openedCount += 2;
            CurrentTransaction?.Dispose();
            CurrentTransaction = null;
            EnlistedTransaction = null;
            if (_ambientTransaction != null)
            {
                _ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
                _ambientTransaction = null;
            }

            _openedCount = previousOpenedCount;
        }

        private void OpenDbConnection(bool errorsExpected)
        {
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            Dependencies.ConnectionLogger.ConnectionOpening(
                this,
                startTime,
                async: false);

            try
            {
                DbConnection.Open();

                Dependencies.ConnectionLogger.ConnectionOpened(
                    this,
                    startTime,
                    stopwatch.Elapsed,
                    async: false);
            }
            catch (Exception e)
            {
                Dependencies.ConnectionLogger.ConnectionError(
                    this,
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    async: false,
                    logErrorAsDebug: errorsExpected);

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

            Dependencies.ConnectionLogger.ConnectionOpening(
                this,
                startTime,
                async: true);

            try
            {
                await DbConnection.OpenAsync(cancellationToken);

                Dependencies.ConnectionLogger.ConnectionOpened(
                    this,
                    startTime,
                    stopwatch.Elapsed,
                    async: true);
            }
            catch (Exception e)
            {
                Dependencies.ConnectionLogger.ConnectionError(
                    this,
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    async: true,
                    logErrorAsDebug: errorsExpected);

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

            if (current != null)
            {
                Dependencies.TransactionLogger.AmbientTransactionEnlisted(this, current);
            }

            DbConnection.EnlistTransaction(current);
            try
            {
                _semaphore.Wait();

                if (_ambientTransaction != null)
                {
                    _ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
                    _openedCount--;
                }

                if (current != null)
                {
                    _openedCount++;
                    current.TransactionCompleted += HandleTransactionCompleted;
                }

                _ambientTransaction = current;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void HandleTransactionCompleted(object sender, TransactionEventArgs e)
        {
            // This could be invoked on a different thread at arbitrary time after the transaction completes
            try
            {
                _semaphore.Wait();

                if (_ambientTransaction != null)
                {
                    _ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
                    _ambientTransaction = null;
                    _openedCount--;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns> True if the underlying connection was actually closed; false otherwise. </returns>
        public virtual bool Close()
        {
            var wasClosed = false;

            if ((_openedCount == 0
                 || _openedCount > 0
                 && --_openedCount == 0)
                && _openedInternally)
            {
                if (DbConnection.State != ConnectionState.Closed)
                {
                    var startTime = DateTimeOffset.UtcNow;
                    var stopwatch = Stopwatch.StartNew();

                    Dependencies.ConnectionLogger.ConnectionClosing(this, startTime);

                    try
                    {
                        DbConnection.Close();

                        wasClosed = true;

                        Dependencies.ConnectionLogger.ConnectionClosed(this, startTime, stopwatch.Elapsed);
                    }
                    catch (Exception e)
                    {
                        Dependencies.ConnectionLogger.ConnectionError(
                            this,
                            e,
                            startTime,
                            stopwatch.Elapsed,
                            async: false,
                            logErrorAsDebug: false);

                        throw;
                    }
                }

                _openedInternally = false;
            }

            return wasClosed;
        }

        /// <summary>
        ///     Gets a value indicating whether the multiple active result sets feature is enabled.
        /// </summary>
        public virtual bool IsMultipleActiveResultSetsEnabled => false;

        void IResettableService.ResetState() => Dispose();

        /// <summary>
        ///     Gets a semaphore used to serialize access to this connection.
        /// </summary>
        /// <value>
        ///     The semaphore used to serialize access to this connection.
        /// </value>
        public virtual SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

        private readonly List<IBufferable> _activeQueries = new List<IBufferable>();
        private Transaction _enlistedTransaction;

        /// <summary>
        ///     Registers a potentially bufferable active query.
        /// </summary>
        /// <param name="bufferable"> The bufferable query. </param>
        void IRelationalConnection.RegisterBufferable(IBufferable bufferable)
        {
            // hot path
            Debug.Assert(bufferable != null);

            if (!IsMultipleActiveResultSetsEnabled)
            {
                for (var i = _activeQueries.Count - 1; i >= 0; i--)
                {
                    _activeQueries[i].BufferAll();

                    _activeQueries.RemoveAt(i);
                }

                _activeQueries.Add(bufferable);
            }
        }

        /// <summary>
        ///     Unregisters a potentially bufferable active query.
        /// </summary>
        /// <param name="bufferable"> The bufferable query. </param>
        void IRelationalConnection.UnregisterBufferable(IBufferable bufferable)
        {
            // hot path
            Debug.Assert(bufferable != null);

            if (!IsMultipleActiveResultSetsEnabled)
            {
                _activeQueries.Remove(bufferable);
            }
        }

        /// <summary>
        ///     Asynchronously registers a potentially bufferable active query.
        /// </summary>
        /// <param name="bufferable"> The bufferable query. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A Task.
        /// </returns>
        async Task IRelationalConnection.RegisterBufferableAsync(IBufferable bufferable, CancellationToken cancellationToken)
        {
            // hot path
            Debug.Assert(bufferable != null);

            if (!IsMultipleActiveResultSetsEnabled)
            {
                for (var i = _activeQueries.Count - 1; i >= 0; i--)
                {
                    await _activeQueries[i].BufferAllAsync(cancellationToken);

                    _activeQueries.RemoveAt(i);
                }

                _activeQueries.Add(bufferable);
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            ClearTransactions();

            if (_connectionOwned && _connection.HasValue)
            {
                DbConnection.Dispose();
                _connection.Reset(CreateDbConnection);
                _activeQueries.Clear();
                _openedCount = 0;
            }
        }
    }
}
