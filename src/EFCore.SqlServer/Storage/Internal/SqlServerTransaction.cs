// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerTransaction : RelationalTransaction, IDbContextTransaction
    {
        private readonly DbTransaction _dbTransaction;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerTransaction(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            : base(connection, transaction, transactionId, logger, transactionOwned)
            => _dbTransaction = transaction;

        /// <inheritdoc />
        public virtual void Save(string savepointName)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "SAVE TRANSACTION " + savepointName;
            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public virtual async Task SaveAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "SAVE TRANSACTION " + savepointName;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual void Rollback(string savepointName)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "ROLLBACK TRANSACTION " + savepointName;
            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public virtual async Task RollbackAsync(string savepointName, CancellationToken cancellationToken = default)
        {
            using var command = Connection.DbConnection.CreateCommand();
            command.Transaction = _dbTransaction;
            command.CommandText = "ROLLBACK TRANSACTION " + savepointName;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual bool AreSavepointsSupported => true;
    }
}
