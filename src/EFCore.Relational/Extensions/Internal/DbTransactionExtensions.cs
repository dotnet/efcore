// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Common
{
    internal static class DbTransactionExtensions
    {
        private static readonly Func<DbTransaction, CancellationToken, Task> _commitAsync;
        private static readonly Func<DbTransaction, CancellationToken, Task> _rollbackAsync;

        static DbTransactionExtensions()
        {
            var commitAsync = typeof(DbTransaction)
                .GetMethod("CommitAsync", new[] { typeof(CancellationToken) });
            if (commitAsync != null)
            {
                var transaction = Expression.Parameter(typeof(DbTransaction), "transaction");
                var cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                _commitAsync = Expression
                    .Lambda<Func<DbTransaction, CancellationToken, Task>>(
                        Expression.Call(transaction, commitAsync, cancellationToken),
                        transaction,
                        cancellationToken)
                    .Compile();
            }
            else
            {
                _commitAsync = CommitSync;
            }

            var rollbackAsync = typeof(DbTransaction)
                .GetMethod("RollbackAsync", new[] { typeof(CancellationToken) });
            if (rollbackAsync != null)
            {
                var transaction = Expression.Parameter(typeof(DbTransaction), "transaction");
                var cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                _rollbackAsync = Expression
                    .Lambda<Func<DbTransaction, CancellationToken, Task>>(
                        Expression.Call(transaction, rollbackAsync, cancellationToken),
                        transaction,
                        cancellationToken)
                    .Compile();
            }
            else
            {
                _rollbackAsync = RollbackSync;
            }
        }

        public static Task CommitAsync(this DbTransaction transaction, CancellationToken cancellationToken)
            => _commitAsync(transaction, cancellationToken);

        private static Task CommitSync(DbTransaction transaction, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                transaction.Commit();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public static Task RollbackAsync(this DbTransaction transaction, CancellationToken cancellationToken)
            => _rollbackAsync(transaction, cancellationToken);

        private static Task RollbackSync(DbTransaction transaction, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                transaction.Rollback();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }
    }
}
