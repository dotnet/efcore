// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommand : IRelationalCommand
    {
        private readonly LazyRef<ILogger> _logger;
        private IRelationalTypeMapper _typeMapper;

        public RelationalCommand(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<RelationalParameter> parameters)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(commandText, nameof(commandText));
            Check.NotNull(parameters, nameof(parameters));

            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<RelationalCommand>);
            _typeMapper = typeMapper;
            CommandText = commandText;
            Parameters = parameters;
        }

        protected virtual ILogger Logger => _logger.Value;

        public virtual string CommandText { get; }

        public virtual IReadOnlyList<RelationalParameter> Parameters { get; }

        public virtual void ExecuteNonQuery([NotNull] IRelationalConnection connection)
            => Execute<object>(connection, c => c.ExecuteNonQuery());

        public virtual object ExecuteScalar([NotNull] IRelationalConnection connection)
            => Execute(connection, c => c.ExecuteScalar());

        public virtual DbDataReader ExecuteReader([NotNull] IRelationalConnection connection)
            => Execute(connection, c => c.ExecuteReader());

        public virtual Task ExecuteNonQueryAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync<object>(connection, async c => await c.ExecuteNonQueryAsync(), cancellationToken);

        public virtual Task<object> ExecuteScalarAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(connection, async c => await c.ExecuteScalarAsync(), cancellationToken);

        public virtual Task<DbDataReader> ExecuteReaderAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
            => ExecuteAsync(connection, async c => await c.ExecuteReaderAsync(), cancellationToken);

        protected virtual T Execute<T>(
            [NotNull] IRelationalConnection connection,
            [NotNull] Func<DbCommand, T> action)
        {
            connection.Open();

            try
            {
                using (var dbCommand = CreateCommand(connection))
                {
                    if (Logger.IsEnabled(LogLevel.Verbose))
                    {
                        Logger.LogCommand(dbCommand);
                    }

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
            [NotNull] Func<DbCommand, Task<T>> action,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await connection.OpenAsync(cancellationToken);

            try
            {
                using (var dbCommand = CreateCommand(connection))
                {
                    if (Logger.IsEnabled(LogLevel.Verbose))
                    {
                        Logger.LogCommand(dbCommand);
                    }

                    return await action(dbCommand);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        protected virtual DbCommand CreateCommand([NotNull] IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var command = connection.DbConnection.CreateCommand();
            command.CommandText = CommandText;

            if (connection.Transaction != null)
            {
                command.Transaction = connection.Transaction.GetService();
            }

            if (connection.CommandTimeout.HasValue)
            {
                command.CommandTimeout = connection.CommandTimeout.Value;
            }

            foreach (var parameter in Parameters)
            {
                command.Parameters.Add(
                    parameter.Property == null
                        ? _typeMapper.GetMapping(parameter.Value)
                            .CreateParameter(
                                command,
                                parameter.Name,
                                parameter.Value)
                        : _typeMapper.GetMapping(parameter.Property)
                            .CreateParameter(
                                command,
                                parameter.Name,
                                parameter.Value,
                                parameter.Property.IsNullable));
            }

            return command;
        }
    }
}
