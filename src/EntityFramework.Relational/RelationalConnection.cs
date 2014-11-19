// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalConnection : DataStoreConnection, IDisposable
    {
        private readonly string _connectionString;
        private readonly LazyRef<DbConnection> _connection;
        private readonly bool _connectionOwned;
        private int _openedCount;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RelationalConnection()
        {
        }

        protected RelationalConnection([NotNull] LazyRef<IDbContextOptions> options, [NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Check.NotNull(options, "options");

            var storeConfig = RelationalOptionsExtension.Extract(options.Value);

            if (storeConfig.Connection != null)
            {
                if (!string.IsNullOrWhiteSpace(storeConfig.ConnectionString))
                {
                    throw new InvalidOperationException(Strings.ConnectionAndConnectionString);
                }

                _connection = new LazyRef<DbConnection>(() => storeConfig.Connection);
                _connectionOwned = false;
                _openedCount = storeConfig.Connection.State == ConnectionState.Open ? 1 : 0;
            }
            else if (!string.IsNullOrWhiteSpace(storeConfig.ConnectionString))
            {
                _connectionString = storeConfig.ConnectionString;
                _connection = new LazyRef<DbConnection>(CreateDbConnection);
                _connectionOwned = true;
            }
            else
            {
                throw new InvalidOperationException(Strings.NoConnectionOrConnectionString);
            }
        }

        protected abstract DbConnection CreateDbConnection();

        public virtual string ConnectionString
        {
            get { return _connectionString ?? _connection.Value.ConnectionString; }
        }

        public virtual DbConnection DbConnection
        {
            get { return _connection.Value; }
        }

        public virtual RelationalTransaction Transaction { get; protected set; }

        public virtual DbTransaction DbTransaction
        {
            get
            {
                return Transaction == null
                    ? null
                    : Transaction.DbTransaction;
            }
        }

        [NotNull]
        public virtual RelationalTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        [NotNull]
        public virtual Task<RelationalTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);
        }

        [NotNull]
        public virtual RelationalTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (Transaction != null)
            {
                throw new InvalidOperationException(Strings.TransactionAlreadyStarted);
            }

            Open();

            return BeginTransactionWithNoPreconditions(isolationLevel);
        }

        [NotNull]
        public virtual async Task<RelationalTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Transaction != null)
            {
                throw new InvalidOperationException(Strings.TransactionAlreadyStarted);
            }

            await OpenAsync(cancellationToken).WithCurrentCulture();

            return BeginTransactionWithNoPreconditions(isolationLevel);
        }

        private RelationalTransaction BeginTransactionWithNoPreconditions(IsolationLevel isolationLevel)
        {
            Logger.BeginningTransaction(isolationLevel);

            Transaction = new RelationalTransaction(this, DbConnection.BeginTransaction(isolationLevel), /*transactionOwned*/ true, Logger);

            return Transaction;
        }

        public virtual RelationalTransaction UseTransaction([CanBeNull] DbTransaction transaction)
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

                Transaction = new RelationalTransaction(this, transaction, /*transactionOwned*/ false, Logger);
            }

            return Transaction;
        }

        public virtual void Open()
        {
            if (_openedCount == 0)
            {
                _connection.Value.Open();
            }
            _openedCount++;
        }

        public virtual async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_openedCount == 0)
            {
                await _connection.Value.OpenAsync(cancellationToken).WithCurrentCulture();
            }
            _openedCount++;
        }

        // Sporadic failure when running Async query tests
        public virtual void Close()
        {
            // TODO: Consider how to handle open/closing to make sure that a connection that is passed in
            // as open is never erroneously closed without placing undue burdon on users of the connection.
            // Disabled: See GitHub #141
            // Debug.Assert(_openedCount > 0);
            if (--_openedCount == 0)
            {
                _connection.Value.Close();
            }
        }

        public virtual void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
            }

            if (_connectionOwned && _connection.HasValue)
            {
                _connection.Value.Dispose();
                _connection.Reset(CreateDbConnection);
                _openedCount = 0;
            }
        }
    }
}
