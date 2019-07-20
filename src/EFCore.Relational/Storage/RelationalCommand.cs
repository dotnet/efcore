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
        ///         Constructs a new <see cref="RelationalCommand" />.
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
        /// <param name="parameterObject"> Parameters for this method. </param>
        /// <returns> The number of rows affected. </returns>
        public virtual int ExecuteNonQuery(RelationalCommandParameterObject parameterObject)
        {
            var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

            var command = CreateCommand(connection, parameterObject.ParameterValues);

            connection.Open();

            var commandId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var interceptionResult = logger?.CommandNonQueryExecuting(
                                             command,
                                             context,
                                             commandId,
                                             connection.ConnectionId,
                                             startTime)
                                         ?? default;

                var nonQueryResult = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : command.ExecuteNonQuery();

                return logger?.CommandNonQueryExecuted(
                           command,
                           context,
                           commandId,
                           connection.ConnectionId,
                           nonQueryResult,
                           startTime,
                           stopwatch.Elapsed)
                       ?? nonQueryResult;
            }
            catch (Exception exception)
            {
                logger?.CommandError(
                    command,
                    context,
                    DbCommandMethod.ExecuteNonQuery,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
            finally
            {
                CleanupCommand(command, connection);
            }
        }

        private static void CleanupCommand(
            DbCommand command,
            IRelationalConnection connection)
        {
            command.Parameters.Clear();
            command.Dispose();
            connection.Close();
        }

        private static async Task CleanupCommandAsync(
            DbCommand command,
            IRelationalConnection connection)
        {
            command.Parameters.Clear();
            await command.DisposeAsync();
            await connection.CloseAsync();
        }

        /// <summary>
        ///     Asynchronously executes the command with no results.
        /// </summary>
        /// <param name="parameterObject"> Parameters for this method. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the number of rows affected.
        /// </returns>
        public virtual async Task<int> ExecuteNonQueryAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken = default)
        {
            var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

            var command = CreateCommand(connection, parameterObject.ParameterValues);

            await connection.OpenAsync(cancellationToken);

            var commandId = Guid.NewGuid();

            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var interceptionResult = logger == null
                    ? default
                    : await logger.CommandNonQueryExecutingAsync(
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        startTime,
                        cancellationToken);

                var result = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : await command.ExecuteNonQueryAsync(cancellationToken);

                if (logger != null)
                {
                    result = await logger.CommandNonQueryExecutedAsync(
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        result,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken);
                }

                return result;
            }
            catch (Exception exception)
            {
                if (logger != null)
                {
                    await logger.CommandErrorAsync(
                        command,
                        context,
                        DbCommandMethod.ExecuteNonQuery,
                        commandId,
                        connection.ConnectionId,
                        exception,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken);
                }

                throw;
            }
            finally
            {
                await CleanupCommandAsync(command, connection);
            }
        }

        /// <summary>
        ///     Executes the command with a single scalar result.
        /// </summary>
        /// <param name="parameterObject"> Parameters for this method. </param>
        /// <returns> The result of the command. </returns>
        public virtual object ExecuteScalar(RelationalCommandParameterObject parameterObject)
        {
            var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

            var command = CreateCommand(connection, parameterObject.ParameterValues);

            connection.Open();

            var commandId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var interceptionResult = logger?.CommandScalarExecuting(
                                             command,
                                             context,
                                             commandId,
                                             connection.ConnectionId,
                                             startTime)
                                         ?? default;

                var result = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : command.ExecuteScalar();

                return logger?.CommandScalarExecuted(
                           command,
                           context,
                           commandId,
                           connection.ConnectionId,
                           result,
                           startTime,
                           stopwatch.Elapsed)
                       ?? result;
            }
            catch (Exception exception)
            {
                logger?.CommandError(
                    command,
                    context,
                    DbCommandMethod.ExecuteScalar,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
            finally
            {
                CleanupCommand(command, connection);
            }
        }

        /// <summary>
        ///     Asynchronously executes the command with a single scalar result.
        /// </summary>
        /// <param name="parameterObject"> Parameters for this method. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the result of the command.
        /// </returns>
        public virtual async Task<object> ExecuteScalarAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken = default)
        {
            var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

            var command = CreateCommand(connection, parameterObject.ParameterValues);

            await connection.OpenAsync(cancellationToken);

            var commandId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var interceptionResult = logger == null
                    ? default
                    : await logger.CommandScalarExecutingAsync(
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        startTime,
                        cancellationToken);

                var result = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : await command.ExecuteScalarAsync(cancellationToken);

                if (logger != null)
                {
                    result = await logger.CommandScalarExecutedAsync(
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        result,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken);
                }

                return result;
            }
            catch (Exception exception)
            {
                if (logger != null)
                {
                    await logger.CommandErrorAsync(
                        command,
                        context,
                        DbCommandMethod.ExecuteScalar,
                        commandId,
                        connection.ConnectionId,
                        exception,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken);
                }

                throw;
            }
            finally
            {
                await CleanupCommandAsync(command, connection);
            }
        }

        /// <summary>
        ///     Executes the command with a <see cref="RelationalDataReader" /> result.
        /// </summary>
        /// <param name="parameterObject"> Parameters for this method. </param>
        /// <returns> The result of the command. </returns>
        public virtual RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
        {
            var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

            var command = CreateCommand(connection, parameterObject.ParameterValues);

            connection.Open();

            var commandId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            var readerOpen = false;
            try
            {
                var interceptionResult = logger?.CommandReaderExecuting(
                                             command,
                                             context,
                                             commandId,
                                             connection.ConnectionId,
                                             startTime)
                                         ?? default;

                var reader = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : command.ExecuteReader();

                if (logger != null)
                {
                    reader = logger.CommandReaderExecuted(
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        reader,
                        startTime,
                        stopwatch.Elapsed);
                }

                var result = new RelationalDataReader(
                    connection,
                    command,
                    reader,
                    commandId,
                    logger);

                readerOpen = true;

                return result;
            }
            catch (Exception exception)
            {
                logger?.CommandError(
                    command,
                    context,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    startTime,
                    stopwatch.Elapsed);

                throw;
            }
            finally
            {
                if (!readerOpen)
                {
                    CleanupCommand(command, connection);
                }
            }
        }

        /// <summary>
        ///     Asynchronously executes the command with a <see cref="RelationalDataReader" /> result.
        /// </summary>
        /// <param name="parameterObject"> Parameters for this method. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the result of the command.
        /// </returns>
        public virtual async Task<RelationalDataReader> ExecuteReaderAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken = default)
        {
            var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

            var command = CreateCommand(connection, parameterObject.ParameterValues);

            await connection.OpenAsync(cancellationToken);

            var commandId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            var readerOpen = false;
            try
            {
                var interceptionResult = logger == null
                    ? default
                    : await logger.CommandReaderExecutingAsync(
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        startTime,
                        cancellationToken);

                var reader = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : await command.ExecuteReaderAsync(cancellationToken);

                if (logger != null)
                {
                    reader = await logger.CommandReaderExecutedAsync(
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        reader,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken);
                }

                var result = new RelationalDataReader(
                    connection,
                    command,
                    reader,
                    commandId,
                    logger);

                readerOpen = true;

                return result;
            }
            catch (Exception exception)
            {
                if (logger != null)
                {
                    await logger.CommandErrorAsync(
                        command,
                        context,
                        DbCommandMethod.ExecuteReader,
                        commandId,
                        connection.ConnectionId,
                        exception,
                        startTime,
                        stopwatch.Elapsed,
                        cancellationToken);
                }

                throw;
            }
            finally
            {
                if (!readerOpen)
                {
                    await CleanupCommandAsync(command, connection);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Template method called by the execute methods to
        ///         create a <see cref="DbCommand" /> for the given <see cref="DbConnection" /> and configure
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
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues)
        {
            Check.NotNull(connection, nameof(connection));

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

            if (Parameters != null
                && Parameters.Count > 0)
            {
                if (parameterValues == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.MissingParameterValue(
                            Parameters[0].InvariantName));
                }

                for (var i = 0; i < Parameters.Count; i++)
                {
                    Parameters[i].AddDbParameter(command, parameterValues);
                }
            }

            return command;
        }
    }
}
