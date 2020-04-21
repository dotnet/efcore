// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class BatchExecutor : IBatchExecutor
    {
        private const string SavepointName = "__EFSavePoint";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BatchExecutor([NotNull] ICurrentDbContext currentContext)
        {
            CurrentContext = currentContext;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ICurrentDbContext CurrentContext { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int Execute(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection)
        {
            var rowsAffected = 0;
            var transaction = connection.CurrentTransaction;
            var beganTransaction = false;
            var createdSavepoint = false;
            try
            {
                if (transaction == null
                    && (connection as ITransactionEnlistmentManager)?.EnlistedTransaction == null
                    && Transaction.Current == null
                    && CurrentContext.Context.Database.AutoTransactionsEnabled)
                {
                    transaction = connection.BeginTransaction();
                    beganTransaction = true;
                }
                else
                {
                    connection.Open();

                    if (transaction?.AreSavepointsSupported == true)
                    {
                        transaction.Save(SavepointName);
                        createdSavepoint = true;
                    }
                }

                foreach (var batch in commandBatches)
                {
                    batch.Execute(connection);
                    rowsAffected += batch.ModificationCommands.Count;
                }

                if (beganTransaction)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (createdSavepoint)
                {
                    transaction.Rollback(SavepointName);
                }

                throw;
            }
            finally
            {
                if (createdSavepoint)
                {
                    transaction.Release(SavepointName);
                }
                else if (beganTransaction)
                {
                    transaction.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }

            return rowsAffected;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<int> ExecuteAsync(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default)
        {
            var rowsAffected = 0;
            var transaction = connection.CurrentTransaction;
            var beganTransaction = false;
            var createdSavepoint = false;
            try
            {
                if (transaction == null
                    && (connection as ITransactionEnlistmentManager)?.EnlistedTransaction == null
                    && Transaction.Current == null
                    && CurrentContext.Context.Database.AutoTransactionsEnabled)
                {
                    transaction = await connection.BeginTransactionAsync(cancellationToken);
                    beganTransaction = true;
                }
                else
                {
                    await connection.OpenAsync(cancellationToken);

                    if (transaction?.AreSavepointsSupported == true)
                    {
                        await transaction.SaveAsync(SavepointName, cancellationToken);
                        createdSavepoint = true;
                    }
                }

                foreach (var batch in commandBatches)
                {
                    await batch.ExecuteAsync(connection, cancellationToken);
                    rowsAffected += batch.ModificationCommands.Count;
                }

                if (beganTransaction)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                if (createdSavepoint)
                {
                    await transaction.RollbackAsync(SavepointName, cancellationToken);
                }

                throw;
            }
            finally
            {
                if (createdSavepoint)
                {
                    await transaction.ReleaseAsync(SavepointName, cancellationToken);
                }
                else if (beganTransaction)
                {
                    await transaction.DisposeAsync();
                }
                else
                {
                    await connection.CloseAsync();
                }
            }

            return rowsAffected;
        }
    }
}
