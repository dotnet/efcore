// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Extensions;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class SqlStatementExecutor : ISqlStatementExecutor
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly TelemetrySource _telemetrySource;
        private readonly ISensitiveDataLogger _logger;

        public SqlStatementExecutor(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISensitiveDataLogger<SqlStatementExecutor> logger,
            [NotNull] TelemetrySource telemetrySource)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(telemetrySource, nameof(telemetrySource));

            _commandBuilderFactory = commandBuilderFactory;
            _logger = logger;
            _telemetrySource = telemetrySource;
        }

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
                CreateCommand(sql),
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
                CreateCommand(sql),
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
                CreateCommand(sql),
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
                CreateCommand(sql),
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
                using (var dbCommand = command.CreateCommand(connection))
                {
                    _logger.LogCommand(dbCommand);
                    _telemetrySource.WriteCommand("Microsoft.Data.Entity.BeforeExecuteCommand", dbCommand);

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
                using (var dbCommand = command.CreateCommand(connection))
                {
                    _logger.LogCommand(dbCommand);
                    _telemetrySource.WriteCommand("Microsoft.Data.Entity.BeforeExecuteCommand", dbCommand);

                    return await action(dbCommand);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        private RelationalCommand CreateCommand(string sql)
            => _commandBuilderFactory.Create()
                .Append(sql)
                .BuildRelationalCommand();
    }
}
