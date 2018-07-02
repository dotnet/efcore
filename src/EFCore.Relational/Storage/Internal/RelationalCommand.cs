// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class RelationalCommand : IRelationalCommand
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalCommand(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<IRelationalParameter> parameters)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(commandText, nameof(commandText));
            Check.NotNull(parameters, nameof(parameters));

            Logger = logger;
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Database.Command> Logger { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string CommandText { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IRelationalParameter> Parameters { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int ExecuteNonQuery(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
            => (int)Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteNonQuery,
                parameterValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<int> ExecuteNonQueryAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteNonQuery,
                parameterValues,
                cancellationToken).Cast<object, int>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteScalar,
                parameterValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteScalar,
                parameterValues,
                cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual RelationalDataReader ExecuteReader(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
            => (RelationalDataReader)Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteReader,
                parameterValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<RelationalDataReader> ExecuteReaderAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteReader,
                parameterValues,
                cancellationToken).Cast<object, RelationalDataReader>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual object Execute(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues)
        {
            Check.NotNull(connection, nameof(connection));

            var dbCommand = CreateCommand(connection, parameterValues);

            connection.Open();

            var commandId = Guid.NewGuid();

            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            Logger.CommandExecuting(
                dbCommand,
                executeMethod,
                commandId,
                connection.ConnectionId,
                async: false,
                startTime: startTime);

            object result;
            var readerOpen = false;
            try
            {
                switch (executeMethod)
                {
                    case DbCommandMethod.ExecuteNonQuery:
                    {
                        result = dbCommand.ExecuteNonQuery();

                        break;
                    }
                    case DbCommandMethod.ExecuteScalar:
                    {
                        result = dbCommand.ExecuteScalar();

                        break;
                    }
                    case DbCommandMethod.ExecuteReader:
                    {
                        result
                            = new RelationalDataReader(
                                connection,
                                dbCommand,
                                dbCommand.ExecuteReader(),
                                commandId,
                                Logger);
                        readerOpen = true;

                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                Logger.CommandExecuted(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    result,
                    false,
                    startTime,
                    stopwatch.Elapsed);
            }
            catch (Exception exception)
            {
                Logger.CommandError(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    false,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
            finally
            {
                if (!readerOpen)
                {
                    dbCommand.Parameters.Clear();
                    dbCommand.Dispose();
                    connection.Close();
                }
            }

            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual async Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(connection, nameof(connection));

            var dbCommand = CreateCommand(connection, parameterValues);

            await connection.OpenAsync(cancellationToken);

            var commandId = Guid.NewGuid();

            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            Logger.CommandExecuting(
                dbCommand,
                executeMethod,
                commandId,
                connection.ConnectionId,
                async: true,
                startTime: startTime);

            object result;
            var readerOpen = false;
            try
            {
                switch (executeMethod)
                {
                    case DbCommandMethod.ExecuteNonQuery:
                    {
                        result = await dbCommand.ExecuteNonQueryAsync(cancellationToken);

                        break;
                    }
                    case DbCommandMethod.ExecuteScalar:
                    {
                        result = await dbCommand.ExecuteScalarAsync(cancellationToken);

                        break;
                    }
                    case DbCommandMethod.ExecuteReader:
                    {
                        result = new RelationalDataReader(
                            connection,
                            dbCommand,
                            await dbCommand.ExecuteReaderAsync(cancellationToken),
                            commandId,
                            Logger);
                        readerOpen = true;

                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                Logger.CommandExecuted(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    result,
                    true,
                    startTime,
                    stopwatch.Elapsed);
            }
            catch (Exception exception)
            {
                Logger.CommandError(
                    dbCommand,
                    executeMethod,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    true,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
            finally
            {
                if (!readerOpen)
                {
                    dbCommand.Parameters.Clear();
                    dbCommand.Dispose();
                    connection.Close();
                }
            }

            return result;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual DbCommand CreateCommand(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
        {
            var command = connection.DbConnection.CreateCommand();

            command.CommandText = AdjustCommandText(CommandText);

            ConfigureCommand(command);

            if (connection.CurrentTransaction != null)
            {
                command.Transaction = connection.CurrentTransaction.GetDbTransaction();
            }

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int)connection.CommandTimeout;
            }

            if (Parameters.Count > 0)
            {
                if (parameterValues == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.MissingParameterValue(
                            Parameters[0].InvariantName));
                }

                foreach (var parameter in Parameters)
                {
                    parameter.AddDbParameter(command, parameterValues);
                }
            }

            return command;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ConfigureCommand(DbCommand command)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual string AdjustCommandText(string commandText)
            => commandText;
    }
}
