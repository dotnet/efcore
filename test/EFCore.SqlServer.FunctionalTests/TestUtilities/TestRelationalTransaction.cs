// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestRelationalTransactionFactory : IRelationalTransactionFactory
    {
        public RelationalTransaction Create(
            IRelationalConnection connection,
            DbTransaction transaction,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            => new TestRelationalTransaction(connection, transaction, logger, transactionOwned);
    }

    public class TestRelationalTransaction : RelationalTransaction
    {
        private readonly TestSqlServerConnection _testConnection;

        public TestRelationalTransaction(
            IRelationalConnection connection,
            DbTransaction transaction,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            : base(connection, transaction, logger, transactionOwned)
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
    }
}
