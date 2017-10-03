// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MigrationCommandExecutor : IMigrationCommandExecutor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void ExecuteNonQuery(
            IEnumerable<MigrationCommand> migrationCommands,
            IRelationalConnection connection)
        {
            Check.NotNull(migrationCommands, nameof(migrationCommands));
            Check.NotNull(connection, nameof(connection));

            using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                connection.Open();

                try
                {
                    IDbContextTransaction transaction = null;

                    try
                    {
                        foreach (var command in migrationCommands)
                        {
                            if (transaction == null
                                && !command.TransactionSuppressed)
                            {
                                transaction = connection.BeginTransaction();
                            }

                            if (transaction != null
                                && command.TransactionSuppressed)
                            {
                                transaction.Commit();
                                transaction.Dispose();
                                transaction = null;
                            }

                            command.ExecuteNonQuery(connection);
                        }

                        transaction?.Commit();
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task ExecuteNonQueryAsync(
            IEnumerable<MigrationCommand> migrationCommands,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(migrationCommands, nameof(migrationCommands));
            Check.NotNull(connection, nameof(connection));

            using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                await connection.OpenAsync(cancellationToken);

                try
                {
                    IDbContextTransaction transaction = null;

                    try
                    {
                        foreach (var command in migrationCommands)
                        {
                            if (transaction == null
                                && !command.TransactionSuppressed)
                            {
                                transaction = await connection.BeginTransactionAsync(cancellationToken);
                            }

                            if (transaction != null
                                && command.TransactionSuppressed)
                            {
                                transaction.Commit();
                                transaction.Dispose();
                                transaction = null;
                            }

                            await command.ExecuteNonQueryAsync(connection, cancellationToken: cancellationToken);
                        }

                        transaction?.Commit();
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
