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
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         A command to be executed against a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalCommand : IRelationalCommand
    {
        /// <summary>
        ///     <para>
        ///         Constructs a new <see cref="RelationalCommand"/>.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> Service dependencies. </param>
        /// <param name="commandText"> The text of the command to be executed. </param>
        /// <param name="parameters"> Parameters for the command. </param>
        public RelationalCommand(
            [NotNull] RelationalCommandBuilderDependencies dependencies,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<IRelationalParameter> parameters)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(commandText, nameof(commandText));
            Check.NotNull(parameters, nameof(parameters));

            Dependencies = dependencies;
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        ///     Command building dependencies.
        /// </summary>
        protected virtual RelationalCommandBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Gets the command text to be executed.
        /// </summary>
        public virtual string CommandText { get; }

        /// <summary>
        ///     Gets the parameters for the command.
        /// </summary>
        public virtual IReadOnlyList<IRelationalParameter> Parameters { get; }

        /// <summary>
        ///     Executes the command with no results.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters. </param>
        /// <param name="logger"> The command logger. </param>
        /// <returns> The number of rows affected. </returns>
        public virtual int ExecuteNonQuery(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            => (int)Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteNonQuery,
                parameterValues,
                logger);

        /// <summary>
        ///     Asynchronously executes the command with no results.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters. </param>
        /// <param name="logger"> The command logger. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the number of rows affected.
        /// </returns>
        public virtual Task<int> ExecuteNonQueryAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteNonQuery,
                parameterValues,
                logger,
                cancellationToken).Cast<object, int>();

        /// <summary>
        ///     Executes the command with a single scalar result.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters. </param>
        /// <param name="logger"> The command logger. </param>
        /// <returns> The result of the command. </returns>
        public virtual object ExecuteScalar(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            => Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteScalar,
                parameterValues,
                logger);

        /// <summary>
        ///     Asynchronously executes the command with a single scalar result.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters. </param>
        /// <param name="logger"> The command logger. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the result of the command.
        /// </returns>
        public virtual Task<object> ExecuteScalarAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteScalar,
                parameterValues,
                logger,
                cancellationToken);

        /// <summary>
        ///     Executes the command with a <see cref="RelationalDataReader" /> result.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters. </param>
        /// <param name="logger"> The command logger. </param>
        /// <returns> The result of the command. </returns>
        public virtual RelationalDataReader ExecuteReader(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
            => (RelationalDataReader)Execute(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteReader,
                parameterValues,
                logger);

        /// <summary>
        ///     Asynchronously executes the command with a <see cref="RelationalDataReader" /> result.
        /// </summary>
        /// <param name="connection"> The connection to execute against. </param>
        /// <param name="parameterValues"> The values for the parameters. </param>
        /// <param name="logger"> The command logger. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the result of the command.
        /// </returns>
        public virtual Task<RelationalDataReader> ExecuteReaderAsync(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(
                Check.NotNull(connection, nameof(connection)),
                DbCommandMethod.ExecuteReader,
                parameterValues,
                logger,
                cancellationToken).Cast<object, RelationalDataReader>();

        /// <summary>
        ///    The method called by other methods on this type to execute synchronously.
        /// </summary>
        /// <param name="connection"> The connection to use. </param>
        /// <param name="executeMethod"> The method type. </param>
        /// <param name="parameterValues"> The parameter values. </param>
        /// <param name="logger"> The command logger. </param>
        /// <returns> The result of the execution. </returns>
        protected virtual object Execute(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            Check.NotNull(connection, nameof(connection));

            var dbCommand = CreateCommand(connection, parameterValues);

            connection.Open();

            var commandId = Guid.NewGuid();

            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            object result;
            var readerOpen = false;
            try
            {
                switch (executeMethod)
                {
                    case DbCommandMethod.ExecuteNonQuery:
                        var nonQueryResult = (logger?.CommandNonQueryExecuting(
                                      dbCommand,
                                      commandId,
                                      connection.ConnectionId,
                                      startTime: startTime)
                                  ?? new InterceptionResult<int>(dbCommand.ExecuteNonQuery())).Result;

                        result = logger?.CommandNonQueryExecuted(
                                     dbCommand,
                                     commandId,
                                     connection.ConnectionId,
                                     nonQueryResult,
                                     startTime,
                                     stopwatch.Elapsed)
                                 ?? nonQueryResult;

                        break;
                    case DbCommandMethod.ExecuteScalar:
                        var scalarResult = (logger?.CommandScalarExecuting(
                                      dbCommand,
                                      commandId,
                                      connection.ConnectionId,
                                      startTime: startTime)
                                  ?? new InterceptionResult<object>(dbCommand.ExecuteScalar())).Result;

                        result = logger?.CommandScalarExecuted(
                                     dbCommand,
                                     commandId,
                                     connection.ConnectionId,
                                     scalarResult,
                                     startTime,
                                     stopwatch.Elapsed)
                                 ?? scalarResult;
                        break;
                    case DbCommandMethod.ExecuteReader:
                        var reader = (logger?.CommandReaderExecuting(
                                          dbCommand,
                                          commandId,
                                          connection.ConnectionId,
                                          startTime: startTime)
                                      ?? new InterceptionResult<DbDataReader>(dbCommand.ExecuteReader())).Result;

                        if (logger != null)
                        {
                            reader = logger?.CommandReaderExecuted(
                                dbCommand,
                                commandId,
                                connection.ConnectionId,
                                reader,
                                startTime,
                                stopwatch.Elapsed);
                        }

                        result = new RelationalDataReader(
                            connection,
                            dbCommand,
                            reader,
                            commandId,
                            logger);

                        readerOpen = true;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            catch (Exception exception)
            {
                logger?.CommandError(
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
        ///    The method called by other methods on this type to execute synchronously.
        /// </summary>
        /// <param name="connection"> The connection to use. </param>
        /// <param name="executeMethod"> The method type. </param>
        /// <param name="parameterValues"> The parameter values. </param>
        /// <param name="logger"> The command logger. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result of the execution. </returns>
        protected virtual async Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            [CanBeNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(connection, nameof(connection));

            var dbCommand = CreateCommand(connection, parameterValues);

            await connection.OpenAsync(cancellationToken);

            var commandId = Guid.NewGuid();

            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            object result;
            var readerOpen = false;
            try
            {
                switch (executeMethod)
                {
                    case DbCommandMethod.ExecuteNonQuery:
                        var nonQueryResult = logger == null
                            ? null
                            : await logger.CommandNonQueryExecutingAsync(
                                dbCommand,
                                commandId,
                                connection.ConnectionId,
                                startTime: startTime,
                                cancellationToken);

                        var nonQueryValue = nonQueryResult.HasValue
                            ? nonQueryResult.Value.Result
                            : await dbCommand.ExecuteNonQueryAsync(cancellationToken);

                        if (logger != null)
                        {
                            nonQueryValue = await logger.CommandNonQueryExecutedAsync(
                                dbCommand,
                                commandId,
                                connection.ConnectionId,
                                nonQueryValue,
                                startTime,
                                stopwatch.Elapsed,
                                cancellationToken);
                        }

                        result = nonQueryValue;
                        break;
                    case DbCommandMethod.ExecuteScalar:
                        var scalarResult = logger == null
                            ? null
                            : await logger.CommandScalarExecutingAsync(
                                dbCommand,
                                commandId,
                                connection.ConnectionId,
                                startTime: startTime,
                                cancellationToken);

                        var scalarValue = scalarResult.HasValue
                            ? scalarResult.Value.Result
                            : await dbCommand.ExecuteScalarAsync(cancellationToken);

                        if (logger != null)
                        {
                            scalarValue = await logger.CommandScalarExecutedAsync(
                                dbCommand,
                                commandId,
                                connection.ConnectionId,
                                scalarValue,
                                startTime,
                                stopwatch.Elapsed,
                                cancellationToken);
                        }

                        result = scalarValue;
                        break;
                    case DbCommandMethod.ExecuteReader:
                        var readerResult = logger == null
                            ? null
                            : await logger.CommandReaderExecutingAsync(
                                dbCommand,
                                commandId,
                                connection.ConnectionId,
                                startTime: startTime,
                                cancellationToken);

                        var reader = readerResult.HasValue
                            ? readerResult.Value.Result
                            : await dbCommand.ExecuteReaderAsync(cancellationToken);

                        if (logger != null)
                        {
                            reader = await logger.CommandReaderExecutedAsync(
                                dbCommand,
                                commandId,
                                connection.ConnectionId,
                                reader,
                                startTime,
                                stopwatch.Elapsed,
                                cancellationToken);
                        }

                        readerOpen = true;

                        result = new RelationalDataReader(
                            connection,
                            dbCommand,
                            reader,
                            commandId,
                            logger);

                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            catch (Exception exception)
            {
                logger?.CommandError(
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
        ///     <para>
        ///         Template method called by <see cref="Execute"/> amd <see cref="ExecuteAsync"/> to
        ///         create a <see cref="DbCommand"/> for the given <see cref="DbConnection"/> and configure
        ///         timeouts and transactions.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="parameterValues"> The parameter values. </param>
        /// <returns> The created command. </returns>
        protected virtual DbCommand CreateCommand(
            [NotNull] IRelationalConnection connection,
            [NotNull] IReadOnlyDictionary<string, object> parameterValues)
        {
            var command = connection.DbConnection.CreateCommand();

            command.CommandText = CommandText;

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
    }
}
