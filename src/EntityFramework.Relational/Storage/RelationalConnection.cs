// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class RelationalConnection : IRelationalConnection
    {
        private readonly string _connectionString;
        private readonly LazyRef<DbConnection> _connection;
        private readonly bool _connectionOwned;
        private int _openedCount;
        private int? _commandTimeout;
        private readonly LazyRef<ILogger> _logger;
#if NET45
        private readonly bool _throwOnAmbientTransaction;
#endif

        protected RelationalConnection([NotNull] IDbContextOptions options, [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<RelationalConnection>);

            var config = RelationalOptionsExtension.Extract(options);

            _commandTimeout = config.CommandTimeout;

            if (config.Connection != null)
            {
                if (!string.IsNullOrWhiteSpace(config.ConnectionString))
                {
                    throw new InvalidOperationException(Strings.ConnectionAndConnectionString);
                }

                _connection = new LazyRef<DbConnection>(() => config.Connection);
                _connectionOwned = false;
                _openedCount = config.Connection.State == ConnectionState.Open ? 1 : 0;
            }
            else if (!string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                _connectionString = config.ConnectionString;
                _connection = new LazyRef<DbConnection>(CreateDbConnection);
                _connectionOwned = true;
            }
            else
            {
                throw new InvalidOperationException(Strings.NoConnectionOrConnectionString);
            }

#if NET45
            _throwOnAmbientTransaction = config.ThrowOnAmbientTransaction ?? true;
#endif
        }

        protected abstract DbConnection CreateDbConnection();

        public virtual string ConnectionString => _connectionString ?? _connection.Value.ConnectionString;

        public virtual DbConnection DbConnection => _connection.Value;

        public virtual IRelationalTransaction Transaction { get; protected set; }

        public virtual int? CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (value.HasValue
                    && value < 0)
                {
                    throw new ArgumentException(Strings.InvalidCommandTimeout);
                }

                _commandTimeout = value;
            }
        }

        public virtual DbTransaction DbTransaction => Transaction?.DbTransaction;

        [NotNull]
        public virtual IRelationalTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);

        [NotNull]
        public virtual Task<IRelationalTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
            => BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);

        [NotNull]
        public virtual IRelationalTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (Transaction != null)
            {
                throw new InvalidOperationException(Strings.TransactionAlreadyStarted);
            }

            Open();

            return BeginTransactionWithNoPreconditions(isolationLevel);
        }

        [NotNull]
        public virtual async Task<IRelationalTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Transaction != null)
            {
                throw new InvalidOperationException(Strings.TransactionAlreadyStarted);
            }

            await OpenAsync(cancellationToken);

            return BeginTransactionWithNoPreconditions(isolationLevel);
        }

        private IRelationalTransaction BeginTransactionWithNoPreconditions(IsolationLevel isolationLevel)
        {
            _logger.Value.BeginningTransaction(isolationLevel);

            Transaction = new RelationalTransaction(this, DbConnection.BeginTransaction(isolationLevel), /*transactionOwned*/ true, _logger.Value);

            return Transaction;
        }

        public virtual IRelationalTransaction UseTransaction(DbTransaction transaction)
        {
            if (transaction == null)
            {
                if (Transaction != null)
                {
                    Transaction = null;

                    Close();
                }
            }
            else
            {
                if (Transaction != null)
                {
                    throw new InvalidOperationException(Strings.TransactionAlreadyStarted);
                }

                Open();

                Transaction = new RelationalTransaction(this, transaction, /*transactionOwned*/ false, _logger.Value);
            }

            return Transaction;
        }

        public virtual void Open()
        {
#if NET45
            CheckForAmbientTransactions();

#endif
            if (_openedCount == 0)
            {
                _connection.Value.Open();
            }

            _openedCount++;
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
#if NET45
            CheckForAmbientTransactions();

#endif
            if (_openedCount == 0)
            {
                await _connection.Value.OpenAsync(cancellationToken);
            }

            _openedCount++;
        }

#if NET45
        private void CheckForAmbientTransactions()
        {
            if (_throwOnAmbientTransaction
                && System.Transactions.Transaction.Current != null)
            {
                throw new InvalidOperationException(Strings.AmbientTransaction);
            }
        }
#endif

        public virtual void Close()
        {
            // TODO: Consider how to handle open/closing to make sure that a connection that is passed in
            // as open is never erroneously closed without placing undue burdon on users of the connection.

            if (_openedCount > 0
                && --_openedCount == 0)
            {
                _connection.Value.Close();
            }
        }

        public virtual bool IsMultipleActiveResultSetsEnabled => false;

        public virtual void Dispose()
        {
            Transaction?.Dispose();

            if (_connectionOwned && _connection.HasValue)
            {
                _connection.Value.Dispose();
                _connection.Reset(CreateDbConnection);
                _openedCount = 0;
            }
        }
    }
}
