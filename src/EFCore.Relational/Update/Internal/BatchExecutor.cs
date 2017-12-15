// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class BatchExecutor : IBatchExecutor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public BatchExecutor([NotNull] ICurrentDbContext currentContext, [NotNull] IExecutionStrategyFactory executionStrategyFactory)
        {
            CurrentContext = currentContext;
            ExecutionStrategyFactory = executionStrategyFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ICurrentDbContext CurrentContext { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IExecutionStrategyFactory ExecutionStrategyFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int Execute(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection)
            => CurrentContext.Context.Database.AutoTransactionsEnabled
                ? ExecutionStrategyFactory.Create().Execute((commandBatches, connection), Execute, null)
                : Execute(CurrentContext.Context, (commandBatches, connection));

        private int Execute(DbContext _, (IEnumerable<ModificationCommandBatch>, IRelationalConnection) parameters)
        {
            var commandBatches = parameters.Item1;
            var connection = parameters.Item2;
            var rowsAffected = 0;
            IDbContextTransaction startedTransaction = null;
            try
            {
                if (connection.CurrentTransaction == null
                    && (connection as ITransactionEnlistmentManager)?.EnlistedTransaction == null
                    && Transaction.Current == null
                    && CurrentContext.Context.Database.AutoTransactionsEnabled)
                {
                    startedTransaction = connection.BeginTransaction();
                }
                else
                {
                    connection.Open();
                }

                foreach (var batch in commandBatches)
                {
                    batch.Execute(connection);
                    rowsAffected += batch.ModificationCommands.Count;
                }

                startedTransaction?.Commit();
            }
            finally
            {
                if (startedTransaction != null)
                {
                    startedTransaction.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }

            return rowsAffected;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<int> ExecuteAsync(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default)
            => CurrentContext.Context.Database.AutoTransactionsEnabled
                ? ExecutionStrategyFactory.Create().ExecuteAsync((commandBatches, connection), ExecuteAsync, null, cancellationToken)
                : ExecuteAsync(CurrentContext.Context, (commandBatches, connection), cancellationToken);

        private async Task<int> ExecuteAsync(
            DbContext _,
            (IEnumerable<ModificationCommandBatch>, IRelationalConnection) parameters,
            CancellationToken cancellationToken = default)
        {
            var commandBatches = parameters.Item1;
            var connection = parameters.Item2;
            var rowsAffected = 0;
            IDbContextTransaction startedTransaction = null;
            try
            {
                if (connection.CurrentTransaction == null
                    && (connection as ITransactionEnlistmentManager)?.EnlistedTransaction == null
                    && Transaction.Current == null
                    && CurrentContext.Context.Database.AutoTransactionsEnabled)
                {
                    startedTransaction = await connection.BeginTransactionAsync(cancellationToken);
                }
                else
                {
                    await connection.OpenAsync(cancellationToken);
                }

                foreach (var batch in commandBatches)
                {
                    await batch.ExecuteAsync(connection, cancellationToken);
                    rowsAffected += batch.ModificationCommands.Count;
                }

                startedTransaction?.Commit();
            }
            finally
            {
                if (startedTransaction != null)
                {
                    startedTransaction.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }

            return rowsAffected;
        }
    }
}
