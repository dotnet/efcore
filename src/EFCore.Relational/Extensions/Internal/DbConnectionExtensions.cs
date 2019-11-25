// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Common
{
    internal static class DbConnectionExtensions
    {
        private static readonly Func<DbConnection, IsolationLevel, CancellationToken, ValueTask<DbTransaction>> _beginTransactionAsync;
        private static readonly Func<DbConnection, Task> _closeAsync;

        static DbConnectionExtensions()
        {
            var beginTransactionAsync = typeof(DbConnection)
                .GetMethod("BeginTransactionAsync", new[] { typeof(IsolationLevel), typeof(CancellationToken) });
            if (beginTransactionAsync != null)
            {
                var connection = Expression.Parameter(typeof(DbConnection), "connection");
                var isolationLevel = Expression.Parameter(typeof(IsolationLevel), "isolationLevel");
                var cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                _beginTransactionAsync = Expression
                    .Lambda<Func<DbConnection, IsolationLevel, CancellationToken, ValueTask<DbTransaction>>>(
                        Expression.Call(connection, beginTransactionAsync, isolationLevel, cancellationToken),
                        connection,
                        isolationLevel,
                        cancellationToken)
                    .Compile();
            }
            else
            {
                _beginTransactionAsync = BeginTransactionSync;
            }

            var closeAsync = typeof(DbConnection).GetMethod("CloseAsync", Type.EmptyTypes);
            if (closeAsync != null)
            {
                var connection = Expression.Parameter(typeof(DbConnection), "connection");

                _closeAsync = Expression
                    .Lambda<Func<DbConnection, Task>>(Expression.Call(connection, closeAsync), connection)
                    .Compile();
            }
            else
            {
                _closeAsync = CloseSync;
            }
        }

        public static ValueTask<DbTransaction> BeginTransactionAsync(
            this DbConnection connection,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken)
            => _beginTransactionAsync(connection, isolationLevel, cancellationToken);

        private static ValueTask<DbTransaction> BeginTransactionSync(
            DbConnection connection,
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask<DbTransaction>(Task.FromCanceled<DbTransaction>(cancellationToken));
            }

            try
            {
                return new ValueTask<DbTransaction>(connection.BeginTransaction(isolationLevel));
            }
            catch (Exception ex)
            {
                return new ValueTask<DbTransaction>(Task.FromException<DbTransaction>(ex));
            }
        }

        public static Task CloseAsync(this DbConnection connection)
            => _closeAsync(connection);

        private static Task CloseSync(DbConnection connection)
        {
            try
            {
                connection.Close();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }
    }
}
