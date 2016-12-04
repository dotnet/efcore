// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities
{
    public class TestSqlServerConnection : ISqlServerConnection
    {
        private readonly ISqlServerConnection _realConnection;

        public TestSqlServerConnection(IDbContextOptions options, ILogger<SqlServerConnection> logger)
            : this(new SqlServerConnection(options, logger))
        {
        }

        protected TestSqlServerConnection(ISqlServerConnection connection)
        {
            _realConnection = connection;
        }

        public int ErrorNumber { get; set; } = -2;
        public Queue<bool?> OpenFailures { get; } = new Queue<bool?>();
        public int OpenCount { get; set; }
        public Queue<bool?> CommitFailures { get; } = new Queue<bool?>();
        public Queue<bool?> ExecutionFailures { get; } = new Queue<bool?>();
        public int ExecutionCount { get; set; }

        public virtual string ConnectionString => _realConnection.ConnectionString;
        public virtual DbConnection DbConnection => _realConnection.DbConnection;

        public virtual int? CommandTimeout
        {
            get { return _realConnection.CommandTimeout; }
            set { _realConnection.CommandTimeout = value; }
        }

        public virtual IDbContextTransaction CurrentTransaction { get; private set; }

        public virtual IValueBufferCursor ActiveCursor
        {
            get { return _realConnection.ActiveCursor; }
            set { _realConnection.ActiveCursor = value; }
        }

        public virtual bool IsMultipleActiveResultSetsEnabled => _realConnection.IsMultipleActiveResultSetsEnabled;

        public virtual void Open()
        {
            PreOpen();

            _realConnection.Open();
        }

        public virtual Task OpenAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            PreOpen();

            return _realConnection.OpenAsync(cancellationToken);
        }

        private void PreOpen()
        {
            if (_realConnection.DbConnection.State == ConnectionState.Open)
            {
                return;
            }

            OpenCount++;
            if (OpenFailures.Count <= 0)
            {
                return;
            }
            var fail = OpenFailures.Dequeue();
            if (fail.HasValue)
            {
                throw SqlExceptionFactory.CreateSqlException(ErrorNumber);
            }
        }

        public virtual void Close() => _realConnection.Close();

        public virtual IDbContextTransaction BeginTransaction()
            => BeginTransaction(IsolationLevel.Unspecified);

        public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
            => BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);

        public virtual IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            Open();
            CurrentTransaction = new TestRelationalTransaction(this, _realConnection.BeginTransaction(isolationLevel));
            return CurrentTransaction;
        }

        public virtual async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel, CancellationToken cancellationToken = new CancellationToken())
        {
            await OpenAsync(cancellationToken);
            CurrentTransaction = new TestRelationalTransaction(this, await _realConnection.BeginTransactionAsync(isolationLevel, cancellationToken));
            return CurrentTransaction;
        }

        public IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            var realTransaction = _realConnection.UseTransaction(transaction);
            if (realTransaction == null)
            {
                CurrentTransaction = null;
                return null;
            }

            Open();
            CurrentTransaction = new TestRelationalTransaction(this, realTransaction);
            return CurrentTransaction;
        }

        public void CommitTransaction() => CurrentTransaction.Commit();

        public void RollbackTransaction() => CurrentTransaction.Rollback();

        public void Dispose()
        {
            CurrentTransaction?.Dispose();

            _realConnection.Dispose();
        }

        public ISqlServerConnection CreateMasterConnection() => new TestSqlServerConnection(_realConnection.CreateMasterConnection());
    }
}
