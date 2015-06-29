// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
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
                            var command = CreateCommand(connection, transaction, sqlBatch.Sql);
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
                        var command = CreateCommand(connection, transaction, sql);
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

            var connectionWasOpen = connection.DbConnection.State == ConnectionState.Open;
            if (!connectionWasOpen)
            {
                Logger.OpeningConnection(connection.ConnectionString);

                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                return await action();
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
                            var command = CreateCommand(connection, transaction, sqlBatch.Sql);
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
                        var command = CreateCommand(connection, transaction, sql);
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
