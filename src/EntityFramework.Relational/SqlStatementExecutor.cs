// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public class SqlStatementExecutor
    {
        private readonly LazyRef<ILogger> _logger;

        public SqlStatementExecutor([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<SqlStatementExecutor>);
        }

        protected virtual ILogger Logger => _logger.Value;

        public virtual Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] IEnumerable<SqlBatch> sqlBatches,
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
                            Logger.WriteSql(sqlBatch.Sql);

                            await CreateCommand(connection, transaction, sqlBatch.Sql).ExecuteNonQueryAsync(cancellationToken)
                                .WithCurrentCulture();
                        }
                        return Task.FromResult<object>(null);
                    },
                cancellationToken);
        }

        public virtual Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] string sql,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sql, nameof(sql));

            return ExecuteAsync(
                connection,
                () =>
                    {
                        Logger.WriteSql(sql);

                        return CreateCommand(connection, transaction, sql).ExecuteScalarAsync(cancellationToken);
                    },
                cancellationToken);
        }

        public virtual async Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<Task<object>> action,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));

            // TODO Deal with suppressing transactions etc.

            var connectionWasOpen = connection.DbConnection.State == ConnectionState.Open;
            if (!connectionWasOpen)
            {
                Logger.OpeningConnection(connection.ConnectionString);

                await connection.OpenAsync(cancellationToken).WithCurrentCulture();
            }

            try
            {
                return await action().WithCurrentCulture();
            }
            finally
            {
                if (!connectionWasOpen)
                {
                    Logger.ClosingConnection(connection.ConnectionString);

                    connection.Close();
                }
            }
        }

        public virtual void ExecuteNonQuery(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] IEnumerable<SqlBatch> sqlBatches)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sqlBatches, nameof(sqlBatches));

            Execute(
                connection,
                () =>
                    {
                        foreach (var sqlBatch in sqlBatches)
                        {
                            Logger.WriteSql(sqlBatch.Sql);

                            CreateCommand(connection, transaction, sqlBatch.Sql).ExecuteNonQuery();
                        }
                        return null;
                    });
        }

        public virtual object ExecuteScalar(
            [NotNull] IRelationalConnection connection,
            [CanBeNull] DbTransaction transaction,
            [NotNull] string sql)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(sql, nameof(sql));

            return Execute(
                connection,
                () =>
                    {
                        Logger.WriteSql(sql);

                        return CreateCommand(connection, transaction, sql).ExecuteScalar();
                    });
        }

        public virtual object Execute(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<object> action)
        {
            Check.NotNull(connection, nameof(connection));

            // TODO Deal with suppressing transactions etc.

            var connectionWasOpen = connection.DbConnection.State == ConnectionState.Open;
            if (!connectionWasOpen)
            {
                Logger.OpeningConnection(connection.ConnectionString);

                connection.Open();
            }

            try
            {
                return action();
            }
            finally
            {
                if (!connectionWasOpen)
                {
                    Logger.ClosingConnection(connection.ConnectionString);

                    connection.Close();
                }
            }
        }

        protected virtual DbCommand CreateCommand(
            IRelationalConnection connection,
            DbTransaction transaction,
            string sql)
        {
            var command = connection.DbConnection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            return command;
        }
    }
}
