// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public class SqlStatementExecutor : ISqlStatementExecutor
    {
        private readonly LazyRef<ILogger> _logger;
        private readonly IRelationalTypeMapper _typeMapper;

        public SqlStatementExecutor(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<SqlStatementExecutor>);
            _typeMapper = typeMapper;
        }

        protected virtual ILogger Logger => _logger.Value;

        public virtual void ExecuteNonQuery(
            IRelationalConnection connection,
            IEnumerable<RelationalCommand> relationalCommands)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(relationalCommands, nameof(relationalCommands));

            connection.Open();

            try
            {
                foreach (var command in relationalCommands)
                {
                    Execute<object>(
                        connection,
                        command,
                        c => c.ExecuteNonQuery());
                }
            }
            finally
            {
                connection.Close();
            }
        }

        public virtual async Task ExecuteNonQueryAsync(
            IRelationalConnection connection,
            IEnumerable<RelationalCommand> relationalCommands,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(relationalCommands, nameof(relationalCommands));

            await connection.OpenAsync(cancellationToken);

            try
            {
                foreach (var command in relationalCommands)
                {
                    await ExecuteAsync(
                        connection,
                        command,
                        async c => await c.ExecuteNonQueryAsync(cancellationToken),
                        cancellationToken);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            string sql)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            return Execute(
                connection,
                new RelationalCommand(sql),
                c => c.ExecuteScalar());
        }

        public virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            string sql,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            return ExecuteAsync(
                connection,
                new RelationalCommand(sql),
                c => c.ExecuteScalarAsync(cancellationToken),
                cancellationToken);
        }

        public virtual DbDataReader ExecuteReader(
            IRelationalConnection connection,
            string sql)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            return Execute(
                connection,
                new RelationalCommand(sql),
                c => c.ExecuteReader());
        }

        public virtual Task<DbDataReader> ExecuteReaderAsync(
            IRelationalConnection connection,
            string sql,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotEmpty(sql, nameof(sql));

            return ExecuteAsync(
                connection,
                new RelationalCommand(sql),
                c => c.ExecuteReaderAsync(cancellationToken),
                cancellationToken);
        }

        protected virtual T Execute<T>(
            [NotNull] IRelationalConnection connection,
            [NotNull] RelationalCommand command,
            [NotNull] Func<DbCommand, T> action)
        {
            // TODO Deal with suppressing transactions etc.
            connection.Open();

            try
            {
                using (var dbCommand = command.CreateDbCommand(connection, _typeMapper))
                {
                    Logger.LogCommand(dbCommand);

                    return action(dbCommand);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        protected virtual async Task<T> ExecuteAsync<T>(
            [NotNull] IRelationalConnection connection,
            [NotNull] RelationalCommand command,
            [NotNull] Func<DbCommand, Task<T>> action,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await connection.OpenAsync(cancellationToken);

            try
            {
                using (var dbCommand = command.CreateDbCommand(connection, _typeMapper))
                {
                    Logger.LogCommand(dbCommand);

                    return await action(dbCommand);
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
