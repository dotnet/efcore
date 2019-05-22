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

            logger?.CommandExecuting(
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
                                logger);
                        readerOpen = true;

                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                logger?.CommandExecuted(
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

            logger?.CommandExecuting(
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
                            logger);
                        readerOpen = true;

                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                logger?.CommandExecuted(
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
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
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
