// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class MigrationCommandExecutor : IMigrationCommandExecutor
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ExecuteNonQuery(
            IEnumerable<MigrationCommand> migrationCommands,
            IRelationalConnection connection)
        {
            Check.NotNull(migrationCommands, nameof(migrationCommands));
            Check.NotNull(connection, nameof(connection));

            var userTransaction = connection.CurrentTransaction;
            if (userTransaction is not null && migrationCommands.Any(x => x.TransactionSuppressed))
            {
                throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
            }

            using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                connection.Open();

                try
                {
                    IDbContextTransaction? transaction = null;

                    try
                    {
                        foreach (var command in migrationCommands)
                        {
                            if (transaction == null
                                && !command.TransactionSuppressed
                                && userTransaction is null)
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task ExecuteNonQueryAsync(
            IEnumerable<MigrationCommand> migrationCommands,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(migrationCommands, nameof(migrationCommands));
            Check.NotNull(connection, nameof(connection));

            var userTransaction = connection.CurrentTransaction;
            if (userTransaction is not null && migrationCommands.Any(x => x.TransactionSuppressed))
            {
                throw new NotSupportedException(RelationalStrings.TransactionSuppressedMigrationInUserTransaction);
            }

            var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    IDbContextTransaction? transaction = null;

                    try
                    {
                        foreach (var command in migrationCommands)
                        {
                            if (transaction == null
                                && !command.TransactionSuppressed
                                && userTransaction is null)
                            {
                                transaction = await connection.BeginTransactionAsync(cancellationToken)
                                    .ConfigureAwait(false);
                            }

                            if (transaction != null
                                && command.TransactionSuppressed)
                            {
                                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                                await transaction.DisposeAsync().ConfigureAwait(false);
                                transaction = null;
                            }

                            await command.ExecuteNonQueryAsync(connection, cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
                        }

                        if (transaction != null)
                        {
                            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            await transaction.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await transactionScope.DisposeAsyncIfAvailable().ConfigureAwait(false);
            }
        }
    }
}
