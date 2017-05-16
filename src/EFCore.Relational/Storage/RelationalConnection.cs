// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using IsolationLevel = System.Data.IsolationLevel;

#if NET46
using System.Transactions;
#elif NETSTANDARD1_3
#else
#error target frameworks need to be updated.
#endif

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
    public abstract class RelationalConnection : IRelationalConnection
    {
        private readonly string _connectionString;
        private readonly LazyRef<DbConnection> _connection;
        private readonly bool _connectionOwned;
        private int _openedCount;
        private bool _openedInternally;
        private int? _commandTimeout;

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
                _connectionString = relationalOptions.ConnectionString;
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
        public virtual string ConnectionString => _connectionString ?? _connection.Value.ConnectionString;

        /// <summary>
        ///     Gets the underlying <see cref="System.Data.Common.DbConnection" /> used to connect to the database.
        /// </summary>
        public virtual DbConnection DbConnection => _connection.Value;

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        public virtual IDbContextTransaction CurrentTransaction { get; [param: CanBeNull] protected set; }

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
        public virtual IDbContextTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);

        /// <summary>
        ///     Asynchronously begins a new transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
        /// </returns>
        [NotNull]
        public virtual async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);

        /// <summary>
        ///     Begins a new transaction.
        /// </summary>
        /// <param name="isolationLevel"> The isolation level to use for the transaction. </param>
        /// <returns> The newly created transaction. </returns>
        [NotNull]
        public virtual IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (CurrentTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
            }

            Open();

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
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CurrentTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
            }

            await OpenAsync(cancellationToken);

            return BeginTransactionWithNoPreconditions(isolationLevel);
        }

        private IDbContextTransaction BeginTransactionWithNoPreconditions(IsolationLevel isolationLevel)
        {
            var dbTransaction = DbConnection.BeginTransaction(isolationLevel);

            CurrentTransaction
                = new RelationalTransaction(
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
                if (CurrentTransaction != null)
                {
                    throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
                }

                Open();

                CurrentTransaction = new RelationalTransaction(
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
        /// <returns> True if the underlying connection was actually opened; false otherwise. </returns>
        public virtual bool Open()
        {
            CheckForAmbientTransactions();

            if (_connection.Value.State == ConnectionState.Broken)
            {
                _connection.Value.Close();
            }

            var wasOpened = false;

            if (_connection.Value.State != ConnectionState.Open)
            {
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                Dependencies.ConnectionLogger.ConnectionOpening(
                    this,
                    startTime,
                    async: false);

                try
                {
                    _connection.Value.Open();

                    wasOpened = true;

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
                        async: false);

                    throw;
                }

                if (_openedCount == 0)
                {
                    _openedInternally = true;
                    _openedCount++;
                }
            }
            else
            {
                _openedCount++;
            }

            return wasOpened;
        }

        /// <summary>
        ///     Asynchronously opens the connection to the database.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, with a value of true if the connection
        ///     was actually opened.
        /// </returns>
        public virtual async Task<bool> OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckForAmbientTransactions();

            if (_connection.Value.State == ConnectionState.Broken)
            {
                _connection.Value.Close();
            }

            var wasOpened = false;

            if (_connection.Value.State != ConnectionState.Open)
            {
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                Dependencies.ConnectionLogger.ConnectionOpening(
                    this,
                    startTime,
                    async: true);

                try
                {
                    await _connection.Value.OpenAsync(cancellationToken);

                    wasOpened = true;

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
                        async: true);

                    throw;
                }

                if (_openedCount == 0)
                {
                    _openedInternally = true;
                    _openedCount++;
                }
            }
            else
            {
                _openedCount++;
            }

            return wasOpened;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void CheckForAmbientTransactions()
        {
#if NET46
            if (Transaction.Current != null)
            {
                Dependencies.TransactionLogger.AmbientTransactionWarning(this, DateTimeOffset.UtcNow);
            }
#elif NETSTANDARD1_3
#else
#error target frameworks need to be updated.
#endif
        }

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns> True if the underlying connection was actually closed; false otherwise. </returns>
        public virtual bool Close()
        {
            var wasClosed = false;

            if (_openedCount > 0
                && --_openedCount == 0
                && _openedInternally)
            {
                if (_connection.Value.State != ConnectionState.Closed)
                {
                    var startTime = DateTimeOffset.UtcNow;
                    var stopwatch = Stopwatch.StartNew();

                    Dependencies.ConnectionLogger.ConnectionClosing(this, startTime);

                    try
                    {
                        _connection.Value.Close();

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
                            async: false);

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

        void IResettableService.Reset() => Dispose();

        /// <summary>
        ///     Gets a semaphore used to serialize access to this connection.
        /// </summary>
        /// <value>
        ///     The semaphore used to serialize access to this connection.
        /// </value>
        public virtual SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

        private readonly List<IBufferable> _activeQueries = new List<IBufferable>();

        /// <summary>
        ///     Registers a potentially bufferable active query.
        /// </summary>
        /// <param name="bufferable"> The bufferable query. </param>
        public virtual void RegisterBufferable(IBufferable bufferable)
        {
            Check.NotNull(bufferable, nameof(bufferable));

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
        ///     Asynchronously registers a potentially bufferable active query.
        /// </summary>
        /// <param name="bufferable"> The bufferable query. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A Task.
        /// </returns>
        public virtual async Task RegisterBufferableAsync(IBufferable bufferable, CancellationToken cancellationToken)
        {
            Check.NotNull(bufferable, nameof(bufferable));

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
            CurrentTransaction?.Dispose();

            if (_connectionOwned && _connection.HasValue)
            {
                _connection.Value.Dispose();
                _connection.Reset(CreateDbConnection);
                _activeQueries.Clear();
                _openedCount = 0;
            }
        }
    }
}
