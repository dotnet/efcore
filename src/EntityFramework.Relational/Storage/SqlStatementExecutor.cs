// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
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
                        c => c.ExecuteNonQuery(),
                        RelationalTelemetry.ExecuteMethod.ExecuteNonQuery);
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
                        RelationalTelemetry.ExecuteMethod.ExecuteNonQuery,
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
                c => c.ExecuteScalar(),
                RelationalTelemetry.ExecuteMethod.ExecuteScalar);
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
                RelationalTelemetry.ExecuteMethod.ExecuteScalar,
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
                c => c.ExecuteReader(),
                RelationalTelemetry.ExecuteMethod.ExecuteReader);
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
                RelationalTelemetry.ExecuteMethod.ExecuteReader,
                cancellationToken);
        }

        protected virtual T Execute<T>(
            [NotNull] IRelationalConnection relationalConnection,
            [NotNull] RelationalCommand relationalCommand,
            [NotNull] Func<DbCommand, T> action,
            [NotNull] string executeMethod)
        {
            // TODO Deal with suppressing transactions etc.
            relationalConnection.Open();

            try
            {
                using (var command = relationalCommand.CreateCommand(relationalConnection))
                {
                    _logger.LogCommand(command);

                    WriteTelemetry(
                        RelationalTelemetry.BeforeExecuteCommand,
                        command,
                        executeMethod);

                    T result;

                    try
                    {
                        result = action(command);
                    }
                    catch (Exception exception)
                    {
                        WriteTelemetry(
                            RelationalTelemetry.CommandExecutionError,
                            command,
                            executeMethod,
                            exception: exception);

                        throw;
                    }

                    WriteTelemetry(
                        RelationalTelemetry.AfterExecuteCommand,
                        command,
                        executeMethod);

                    return result;
                }
            }
            finally
            {
                relationalConnection.Close();
            }
        }

        protected virtual async Task<T> ExecuteAsync<T>(
            [NotNull] IRelationalConnection relationalConnection,
            [NotNull] RelationalCommand relationalCommand,
            [NotNull] Func<DbCommand, Task<T>> action,
            [NotNull] string executeMethod,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await relationalConnection.OpenAsync(cancellationToken);

            try
            {
                using (var command = relationalCommand.CreateCommand(relationalConnection))
                {
                    _logger.LogCommand(command);

                    WriteTelemetry(
                        RelationalTelemetry.BeforeExecuteCommand,
                        command,
                        executeMethod,
                        async: true);

                    T result;

                    try
                    {
                        result = await action(command);
                    }
                    catch (Exception exception)
                    {
                        WriteTelemetry(
                            RelationalTelemetry.CommandExecutionError,
                            command,
                            executeMethod,
                            async: true,
                            exception: exception);

                        throw;
                    }

                    WriteTelemetry(
                        RelationalTelemetry.AfterExecuteCommand,
                        command,
                        executeMethod,
                        async: true);

                    return result;
                }
            }
            finally
            {
                relationalConnection.Close();
            }
        }

        private void WriteTelemetry(
            string name, DbCommand command, string executeMethod, bool async = false, Exception exception = null)
            => _telemetrySource
                .WriteCommand(
                    name,
                    command,
                    executeMethod,
                    async: async,
                    exception: exception);

        private RelationalCommand CreateCommand(string sql)
            => _commandBuilderFactory.Create()
                .Append(sql)
                .BuildRelationalCommand();
    }
}
