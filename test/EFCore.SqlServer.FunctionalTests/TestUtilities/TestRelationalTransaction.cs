// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestRelationalTransactionFactory : IRelationalTransactionFactory
    {
        public TestRelationalTransactionFactory(RelationalTransactionFactoryDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        protected virtual RelationalTransactionFactoryDependencies Dependencies { get; }

        public RelationalTransaction Create(
            IRelationalConnection connection,
            DbTransaction transaction,
            Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            => new TestRelationalTransaction(connection, transaction, logger, transactionOwned, Dependencies.SqlGenerationHelper);
    }

    public class TestRelationalTransaction : RelationalTransaction
    {
        private readonly TestSqlServerConnection _testConnection;

        public TestRelationalTransaction(
            IRelationalConnection connection,
            DbTransaction transaction,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned,
            ISqlGenerationHelper sqlGenerationHelper)
            : base(connection, transaction, new Guid(), logger, transactionOwned, sqlGenerationHelper)
        {
            _testConnection = (TestSqlServerConnection)connection;
        }

        public override void Commit()
        {
            if (_testConnection.CommitFailures.Count > 0)
            {
                var fail = _testConnection.CommitFailures.Dequeue();
                if (fail.HasValue)
                {
                    if (fail.Value)
                    {
                        this.GetDbTransaction().Rollback();
                    }
                    else
                    {
                        this.GetDbTransaction().Commit();
                    }

                    _testConnection.DbConnection.Close();
                    throw SqlExceptionFactory.CreateSqlException(_testConnection.ErrorNumber, _testConnection.ConnectionId);
                }
            }

            base.Commit();
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_testConnection.CommitFailures.Count > 0)
            {
                var fail = _testConnection.CommitFailures.Dequeue();
                if (fail.HasValue)
                {
                    if (fail.Value)
                    {
                        await this.GetDbTransaction().RollbackAsync(cancellationToken);
                    }
                    else
                    {
                        await this.GetDbTransaction().CommitAsync(cancellationToken);
                    }

                    await _testConnection.DbConnection.CloseAsync();
                    throw SqlExceptionFactory.CreateSqlException(_testConnection.ErrorNumber, _testConnection.ConnectionId);
                }
            }

            await base.CommitAsync(cancellationToken);
        }

        public override bool SupportsSavepoints
            => true;

        /// <inheritdoc />
        public override void ReleaseSavepoint(string name) { }

        /// <inheritdoc />
        public override Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
