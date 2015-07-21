// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SqlStatementExecutor : ISqlStatementExecutor
    {
        private readonly LazyRef<ILogger> _logger;

        public SqlStatementExecutor([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<SqlStatementExecutor>);
        }

        protected virtual ILogger Logger => _logger.Value;

        public virtual Task ExecuteNonQueryAsync(
            IRelationalConnection connection,
            DbTransaction transaction,
            IEnumerable<SqlBatch> sqlBatches,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sqlBatches, nameof(sqlBatches));

            return ExecuteAsync(
                connection,
                async () =>
                    {
                        foreach (var sqlBatch in sqlBatches)
                        {
                            var command = sqlBatch.CreateCommand(connection, transaction);
                            Logger.LogCommand(command);

                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        return Task.FromResult<object>(null);
                    },
                cancellationToken);
        }

        public virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            DbTransaction transaction,
            string sql,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sql, nameof(sql));

            return ExecuteAsync(
                connection,
                () =>
                    {
                        var command = new SqlBatch(sql).CreateCommand(connection, transaction);
                        Logger.LogCommand(command);

                        return command.ExecuteScalarAsync(cancellationToken);
                    },
                cancellationToken);
        }

        protected virtual async Task<object> ExecuteAsync(
            IRelationalConnection connection,
            Func<Task<object>> action,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));

            await connection.OpenAsync(cancellationToken);

            try
            {
                return await action();
            }
            finally
            {
                connection.Close();
            }
        }

        public virtual void ExecuteNonQuery(
            IRelationalConnection connection,
            DbTransaction transaction,
            IEnumerable<SqlBatch> sqlBatches)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sqlBatches, nameof(sqlBatches));

            Execute(
                connection,
                () =>
                    {
                        foreach (var sqlBatch in sqlBatches)
                        {
                            var command = sqlBatch.CreateCommand(connection, transaction);
                            Logger.LogCommand(command);

                            command.ExecuteNonQuery();
                        }
                        return null;
                    });
        }

        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            DbTransaction transaction,
            string sql)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sql, nameof(sql));

            return Execute(
                connection,
                () =>
                    {
                        var command = new SqlBatch(sql).CreateCommand(connection, transaction);
                        Logger.LogCommand(command);

                        return command.ExecuteScalar();
                    });
        }

        protected virtual object Execute(
            IRelationalConnection connection,
            Func<object> action)
        {
            Check.NotNull(connection, nameof(connection));

            // TODO Deal with suppressing transactions etc.
            connection.Open();

            try
            {
                return action();
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
