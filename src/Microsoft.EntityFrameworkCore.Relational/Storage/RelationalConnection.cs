// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using IsolationLevel = System.Data.IsolationLevel;
#if NET451
using System.Transactions;

#endif
namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalConnection : IRelationalConnection
    {
        private readonly string _connectionString;
        private readonly LazyRef<DbConnection> _connection;
        private readonly bool _connectionOwned;
        private int _openedCount;
        private bool _openedInternally;
        private int? _commandTimeout;
        private readonly ILogger _logger;

        protected RelationalConnection([NotNull] IDbContextOptions options, [NotNull] ILogger logger)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(logger, nameof(logger));

            _logger = logger;

            var relationalOptions = RelationalOptionsExtension.Extract(options);

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

        protected abstract DbConnection CreateDbConnection();

        protected virtual ILogger Logger => _logger;

        public virtual string ConnectionString => _connectionString ?? _connection.Value.ConnectionString;

        public virtual DbConnection DbConnection => _connection.Value;

        public virtual IDbContextTransaction CurrentTransaction { get; [param: NotNull] protected set; }

        public virtual int? CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (value.HasValue
                    && (value < 0))
                {
                    throw new ArgumentException(RelationalStrings.InvalidCommandTimeout);
                }

                _commandTimeout = value;
            }
        }

        [NotNull]
        public virtual IDbContextTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);

        [NotNull]
        public virtual async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);

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
            Check.NotNull(_logger, nameof(_logger));

            _logger.LogDebug(
                RelationalEventId.BeginningTransaction,
                isolationLevel,
                il => RelationalStrings.RelationalLoggerBeginningTransaction(il.ToString("G")));

            CurrentTransaction
                = new RelationalTransaction(
                    this,
                    DbConnection.BeginTransaction(isolationLevel),
                    _logger,
                    transactionOwned: true);

            return CurrentTransaction;
        }

        public virtual IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            if (transaction == null)
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction = null;

                    Close();
                }
            }
            else
            {
                if (CurrentTransaction != null)
                {
                    throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
                }

                Open();

                CurrentTransaction = new RelationalTransaction(this, transaction, _logger, transactionOwned: false);
            }

            return CurrentTransaction;
        }

        public virtual void CommitTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            CurrentTransaction.Commit();
        }

        public virtual void RollbackTransaction()
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            CurrentTransaction.Rollback();
        }

        public virtual void Open()
        {
            CheckForAmbientTransactions();

            if ((_openedCount == 0)
                && (_connection.Value.State != ConnectionState.Open))
            {
                _logger.LogDebug(
                    RelationalEventId.OpeningConnection,
                    new
                    {
                        _connection.Value.Database,
                        _connection.Value.DataSource
                    },
                    state =>
                        RelationalStrings.RelationalLoggerOpeningConnection(
                            state.Database,
                            state.DataSource));

                _connection.Value.Open();
                _openedInternally = true;
            }

            _openedCount++;
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckForAmbientTransactions();

            if ((_openedCount == 0)
                && (_connection.Value.State != ConnectionState.Open))
            {
                _logger.LogDebug(
                    RelationalEventId.OpeningConnection,
                    new
                    {
                        _connection.Value.Database,
                        _connection.Value.DataSource
                    },
                    state =>
                        RelationalStrings.RelationalLoggerOpeningConnection(
                            state.Database,
                            state.DataSource));

                await _connection.Value.OpenAsync(cancellationToken);
                _openedInternally = true;
            }

            _openedCount++;
        }

        private void CheckForAmbientTransactions()
        {
#if NET451
            if (Transaction.Current != null)
            {
                Logger.LogWarning(
                    RelationalEventId.AmbientTransactionWarning,
                    () => RelationalStrings.AmbientTransaction);
            }
#endif
        }

        public virtual void Close()
        {
            // TODO: Consider how to handle open/closing to make sure that a connection that is passed in
            // as open is never erroneously closed without placing undue burdon on users of the connection.

            if ((_openedCount > 0)
                && (--_openedCount == 0)
                && _openedInternally)
            {
                _logger.LogDebug(
                    RelationalEventId.ClosingConnection,
                    new
                    {
                        _connection.Value.Database,
                        _connection.Value.DataSource
                    },
                    state =>
                        RelationalStrings.RelationalLoggerClosingConnection(
                            state.Database,
                            state.DataSource));

                _connection.Value.Close();
                _openedInternally = false;
            }
        }

        public virtual bool IsMultipleActiveResultSetsEnabled => false;

        public virtual IValueBufferCursor ActiveCursor { get; set; }

        public virtual void Dispose()
        {
            CurrentTransaction?.Dispose();

            if (_connectionOwned && _connection.HasValue)
            {
                _connection.Value.Dispose();
                _connection.Reset(CreateDbConnection);
                _openedCount = 0;
            }
        }
    }
}
