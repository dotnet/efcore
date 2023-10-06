// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         A command to be executed against a relational database.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class RelationalCommand : IRelationalCommand
{
    private RelationalDataReader? _relationalReader;

    /// <summary>
    ///     <para>
    ///         Constructs a new <see cref="RelationalCommand" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="dependencies">Service dependencies.</param>
    /// <param name="commandText">The text of the command to be executed.</param>
    /// <param name="parameters">Parameters for the command.</param>
    public RelationalCommand(
        RelationalCommandBuilderDependencies dependencies,
        string commandText,
        IReadOnlyList<IRelationalParameter> parameters)
    {
        Dependencies = dependencies;
        CommandText = commandText;
        Parameters = parameters;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalCommandBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Gets the command text to be executed.
    /// </summary>
    public virtual string CommandText { get; private set; }

    /// <summary>
    ///     Gets the parameters for the command.
    /// </summary>
    public virtual IReadOnlyList<IRelationalParameter> Parameters { get; private set; }

    /// <summary>
    ///     Executes the command with no results.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <returns>The number of rows affected.</returns>
    public virtual int ExecuteNonQuery(RelationalCommandParameterObject parameterObject)
    {
        var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

        var startTime = DateTimeOffset.UtcNow;

        var shouldLogCommandCreate = logger?.ShouldLogCommandCreate(startTime) == true;
        var shouldLogCommandExecute = logger?.ShouldLogCommandExecute(startTime) == true;

        // Guid.NewGuid is expensive, do it only if needed
        var commandId = shouldLogCommandCreate || shouldLogCommandExecute ? Guid.NewGuid() : default;

        var command = CreateDbCommand(parameterObject, commandId, DbCommandMethod.ExecuteNonQuery);

        connection.Open();

        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (shouldLogCommandExecute)
            {
                var interceptionResult = logger?.CommandNonQueryExecuting(
                        connection,
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        startTime,
                        parameterObject.CommandSource)
                    ?? default;

                var nonQueryResult = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : command.ExecuteNonQuery();

                return logger?.CommandNonQueryExecuted(
                        connection,
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        nonQueryResult,
                        startTime,
                        stopwatch.Elapsed,
                        parameterObject.CommandSource)
                    ?? nonQueryResult;
            }
            else
            {
                return command.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            if (Dependencies.ExceptionDetector.IsCancellation(exception))
            {
                logger?.CommandCanceled(
                    connection,
                    command,
                    context,
                    DbCommandMethod.ExecuteNonQuery,
                    commandId,
                    connection.ConnectionId,
                    startTime,
                    stopwatch.Elapsed,
                    parameterObject.CommandSource);
            }
            else
            {
                logger?.CommandError(
                    connection,
                    command,
                    context,
                    DbCommandMethod.ExecuteNonQuery,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    startTime,
                    stopwatch.Elapsed,
                    parameterObject.CommandSource);
            }

            throw;
        }
        finally
        {
            CleanupCommand(command, connection);
        }
    }

    /// <summary>
    ///     Asynchronously executes the command with no results.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<int> ExecuteNonQueryAsync(
        RelationalCommandParameterObject parameterObject,
        CancellationToken cancellationToken = default)
    {
        var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

        var startTime = DateTimeOffset.UtcNow;

        var shouldLogCommandCreate = logger?.ShouldLogCommandCreate(startTime) == true;
        var shouldLogCommandExecute = logger?.ShouldLogCommandExecute(startTime) == true;

        // Guid.NewGuid is expensive, do it only if needed
        var commandId = shouldLogCommandCreate || shouldLogCommandExecute ? Guid.NewGuid() : default;

        var command = CreateDbCommand(parameterObject, commandId, DbCommandMethod.ExecuteNonQuery);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (shouldLogCommandExecute)
            {
                var interceptionResult = logger == null
                    ? default
                    : await logger.CommandNonQueryExecutingAsync(
                            connection,
                            command,
                            context,
                            commandId,
                            connection.ConnectionId,
                            startTime,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);

                var result = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                if (logger != null)
                {
                    result = await logger.CommandNonQueryExecutedAsync(
                            connection,
                            command,
                            context,
                            commandId,
                            connection.ConnectionId,
                            result,
                            startTime,
                            stopwatch.Elapsed,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                return result;
            }
            else
            {
                return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            if (logger != null)
            {
                if (Dependencies.ExceptionDetector.IsCancellation(exception, cancellationToken))
                {
                    await logger.CommandCanceledAsync(
                            connection,
                            command,
                            context,
                            DbCommandMethod.ExecuteNonQuery,
                            commandId,
                            connection.ConnectionId,
                            startTime,
                            stopwatch.Elapsed,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await logger.CommandErrorAsync(
                            connection,
                            command,
                            context,
                            DbCommandMethod.ExecuteNonQuery,
                            commandId,
                            connection.ConnectionId,
                            exception,
                            startTime,
                            stopwatch.Elapsed,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            throw;
        }
        finally
        {
            await CleanupCommandAsync(command, connection).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Executes the command with a single scalar result.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <returns>The result of the command.</returns>
    public virtual object? ExecuteScalar(RelationalCommandParameterObject parameterObject)
    {
        var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

        var startTime = DateTimeOffset.UtcNow;

        var shouldLogCommandCreate = logger?.ShouldLogCommandCreate(startTime) == true;
        var shouldLogCommandExecute = logger?.ShouldLogCommandExecute(startTime) == true;

        // Guid.NewGuid is expensive, do it only if needed
        var commandId = shouldLogCommandCreate || shouldLogCommandExecute ? Guid.NewGuid() : default;

        var command = CreateDbCommand(parameterObject, commandId, DbCommandMethod.ExecuteScalar);

        connection.Open();

        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (shouldLogCommandExecute)
            {
                var interceptionResult = logger?.CommandScalarExecuting(
                        connection,
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        startTime,
                        parameterObject.CommandSource)
                    ?? default;

                var result = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : command.ExecuteScalar();

                return logger?.CommandScalarExecuted(
                        connection,
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        result,
                        startTime,
                        stopwatch.Elapsed,
                        parameterObject.CommandSource)
                    ?? result;
            }
            else
            {
                return command.ExecuteScalar();
            }
        }
        catch (Exception exception)
        {
            if (Dependencies.ExceptionDetector.IsCancellation(exception))
            {
                logger?.CommandCanceled(
                    connection,
                    command,
                    context,
                    DbCommandMethod.ExecuteScalar,
                    commandId,
                    connection.ConnectionId,
                    startTime,
                    stopwatch.Elapsed,
                    parameterObject.CommandSource);
            }
            else
            {
                logger?.CommandError(
                    connection,
                    command,
                    context,
                    DbCommandMethod.ExecuteScalar,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    startTime,
                    stopwatch.Elapsed,
                    parameterObject.CommandSource);
            }

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
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the result of the command.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<object?> ExecuteScalarAsync(
        RelationalCommandParameterObject parameterObject,
        CancellationToken cancellationToken = default)
    {
        var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);

        var startTime = DateTimeOffset.UtcNow;

        var shouldLogCommandCreate = logger?.ShouldLogCommandCreate(startTime) == true;
        var shouldLogCommandExecute = logger?.ShouldLogCommandExecute(startTime) == true;

        // Guid.NewGuid is expensive, do it only if needed
        var commandId = shouldLogCommandCreate || shouldLogCommandExecute ? Guid.NewGuid() : default;

        var command = CreateDbCommand(parameterObject, commandId, DbCommandMethod.ExecuteScalar);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (shouldLogCommandExecute)
            {
                var interceptionResult = logger == null
                    ? default
                    : await logger.CommandScalarExecutingAsync(
                            connection,
                            command,
                            context,
                            commandId,
                            connection.ConnectionId,
                            startTime,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);

                var result = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                if (logger != null)
                {
                    result = await logger.CommandScalarExecutedAsync(
                        connection,
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        result,
                        startTime,
                        stopwatch.Elapsed,
                        parameterObject.CommandSource,
                        cancellationToken).ConfigureAwait(false);
                }

                return result;
            }
            else
            {
                return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            if (logger != null)
            {
                if (Dependencies.ExceptionDetector.IsCancellation(exception, cancellationToken))
                {
                    await logger.CommandCanceledAsync(
                            connection,
                            command,
                            context,
                            DbCommandMethod.ExecuteScalar,
                            commandId,
                            connection.ConnectionId,
                            startTime,
                            stopwatch.Elapsed,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await logger.CommandErrorAsync(
                            connection,
                            command,
                            context,
                            DbCommandMethod.ExecuteScalar,
                            commandId,
                            connection.ConnectionId,
                            exception,
                            startTime,
                            stopwatch.Elapsed,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            throw;
        }
        finally
        {
            await CleanupCommandAsync(command, connection).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Executes the command with a <see cref="RelationalDataReader" /> result.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <returns>The result of the command.</returns>
    public virtual RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
    {
        var connection = parameterObject.Connection;
        var context = parameterObject.Context;
        var readerColumns = parameterObject.ReaderColumns;
        var logger = parameterObject.Logger;
        var detailedErrorsEnabled = parameterObject.DetailedErrorsEnabled;

        var startTime = DateTimeOffset.UtcNow;

        var shouldLogCommandCreate = logger?.ShouldLogCommandCreate(startTime) == true;
        var shouldLogCommandExecute = logger?.ShouldLogCommandExecute(startTime) == true;

        // Guid.NewGuid is expensive, do it only if needed
        var commandId = shouldLogCommandCreate || shouldLogCommandExecute ? Guid.NewGuid() : default;

        connection.Open();

        var command = CreateDbCommand(parameterObject, commandId, DbCommandMethod.ExecuteReader);
        var readerOpen = false;
        DbDataReader reader;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (shouldLogCommandExecute)
            {
                var interceptionResult = logger!.CommandReaderExecuting(
                    connection,
                    command,
                    context,
                    commandId,
                    connection.ConnectionId,
                    startTime,
                    parameterObject.CommandSource);

                reader = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : command.ExecuteReader();

                reader = logger.CommandReaderExecuted(
                    connection,
                    command,
                    context,
                    commandId,
                    connection.ConnectionId,
                    reader,
                    startTime,
                    stopwatch.Elapsed,
                    parameterObject.CommandSource);
            }
            else
            {
                reader = command.ExecuteReader();
            }
        }
        catch (Exception exception)
        {
            if (Dependencies.ExceptionDetector.IsCancellation(exception))
            {
                logger?.CommandCanceled(
                    connection,
                    command,
                    context,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connection.ConnectionId,
                    startTime,
                    stopwatch.Elapsed,
                    parameterObject.CommandSource);
            }
            else
            {
                logger?.CommandError(
                    connection,
                    command,
                    context,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connection.ConnectionId,
                    exception,
                    startTime,
                    stopwatch.Elapsed,
                    parameterObject.CommandSource);
            }

            CleanupCommand(command, connection);

            throw;
        }

        try
        {
            if (readerColumns != null)
            {
                reader = new BufferedDataReader(reader, detailedErrorsEnabled).Initialize(readerColumns);
            }

            _relationalReader ??= CreateRelationalDataReader();

            _relationalReader.Initialize(parameterObject.Connection, command, reader, commandId, logger);

            readerOpen = true;

            return _relationalReader;
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
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the result of the command.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<RelationalDataReader> ExecuteReaderAsync(
        RelationalCommandParameterObject parameterObject,
        CancellationToken cancellationToken = default)
    {
        var connection = parameterObject.Connection;
        var context = parameterObject.Context;
        var readerColumns = parameterObject.ReaderColumns;
        var logger = parameterObject.Logger;
        var detailedErrorsEnabled = parameterObject.DetailedErrorsEnabled;

        var startTime = DateTimeOffset.UtcNow;

        var shouldLogCommandCreate = logger?.ShouldLogCommandCreate(startTime) == true;
        var shouldLogCommandExecute = logger?.ShouldLogCommandExecute(startTime) == true;

        // Guid.NewGuid is expensive, do it only if needed
        var commandId = shouldLogCommandCreate || shouldLogCommandExecute ? Guid.NewGuid() : default;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var command = CreateDbCommand(parameterObject, commandId, DbCommandMethod.ExecuteReader);
        var readerOpen = false;
        DbDataReader reader;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (shouldLogCommandExecute)
            {
                var interceptionResult = await logger!.CommandReaderExecutingAsync(
                        connection,
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        startTime,
                        parameterObject.CommandSource,
                        cancellationToken)
                    .ConfigureAwait(false);

                reader = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                reader = await logger.CommandReaderExecutedAsync(
                        connection,
                        command,
                        context,
                        commandId,
                        connection.ConnectionId,
                        reader,
                        startTime,
                        stopwatch.Elapsed,
                        parameterObject.CommandSource,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            if (logger != null)
            {
                if (Dependencies.ExceptionDetector.IsCancellation(exception, cancellationToken))
                {
                    await logger.CommandCanceledAsync(
                            connection,
                            command,
                            context,
                            DbCommandMethod.ExecuteReader,
                            commandId,
                            connection.ConnectionId,
                            startTime,
                            stopwatch.Elapsed,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await logger.CommandErrorAsync(
                            connection,
                            command,
                            context,
                            DbCommandMethod.ExecuteReader,
                            commandId,
                            connection.ConnectionId,
                            exception,
                            startTime,
                            stopwatch.Elapsed,
                            parameterObject.CommandSource,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            await CleanupCommandAsync(command, connection).ConfigureAwait(false);

            throw;
        }

        try
        {
            if (readerColumns != null)
            {
                reader = await new BufferedDataReader(reader, detailedErrorsEnabled).InitializeAsync(readerColumns, cancellationToken)
                    .ConfigureAwait(false);
            }

            _relationalReader ??= CreateRelationalDataReader();

            _relationalReader.Initialize(parameterObject.Connection, command, reader, commandId, logger);

            readerOpen = true;

            return _relationalReader;
        }
        finally
        {
            if (!readerOpen)
            {
                await CleanupCommandAsync(command, connection).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    ///     <para>
    ///         Called by the execute methods to
    ///         create a <see cref="DbCommand" /> for the given <see cref="DbConnection" /> and configure
    ///         timeouts and transactions.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="commandId">The command correlation ID.</param>
    /// <param name="commandMethod">The method that will be called on the created command.</param>
    /// <returns>The created command.</returns>
    public virtual DbCommand CreateDbCommand(
        RelationalCommandParameterObject parameterObject,
        Guid commandId,
        DbCommandMethod commandMethod)
    {
        var (connection, context, logger) = (parameterObject.Connection, parameterObject.Context, parameterObject.Logger);
        var connectionId = connection.ConnectionId;

        var startTime = DateTimeOffset.UtcNow;

        DbCommand command;

        var stopwatch = SharedStopwatch.StartNew();

        var logCommandCreate = logger?.ShouldLogCommandCreate(startTime) == true;
        if (logCommandCreate)
        {
            var interceptionResult = logger!.CommandCreating(
                connection, commandMethod, context, commandId, connectionId, startTime,
                parameterObject.CommandSource);

            command = interceptionResult.HasResult
                ? interceptionResult.Result
                : connection.DbConnection.CreateCommand();

            command = logger.CommandCreated(
                connection,
                command,
                commandMethod,
                context,
                commandId,
                connectionId,
                startTime,
                stopwatch.Elapsed,
                parameterObject.CommandSource);
        }
        else
        {
            command = connection.DbConnection.CreateCommand();
        }

        command.CommandText = CommandText;

        if (connection.CurrentTransaction != null)
        {
            command.Transaction = connection.CurrentTransaction.GetDbTransaction();
        }

        if (connection.CommandTimeout != null)
        {
            command.CommandTimeout = (int)connection.CommandTimeout;
        }

        for (var i = 0; i < Parameters.Count; i++)
        {
            Parameters[i].AddDbParameter(command, parameterObject.ParameterValues);
        }

        if (logCommandCreate)
        {
            command = logger!.CommandInitialized(
                connection,
                command,
                commandMethod,
                context,
                commandId,
                connectionId,
                startTime,
                stopwatch.Elapsed,
                parameterObject.CommandSource);
        }

        return command;
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
        await command.DisposeAsync().ConfigureAwait(false);
        await connection.CloseAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     <para>
    ///         Creates a new <see cref="RelationalDataReader" /> to be used by <see cref="ExecuteReader" /> and
    ///         <see cref="ExecuteReaderAsync" />. The returned <see cref="RelationalDataReader" /> may get used more for multiple
    ///         queries, and will be re-initialized each time via <see cref="RelationalDataReader.Initialize" />.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <returns>The created <see cref="RelationalDataReader" />.</returns>
    protected virtual RelationalDataReader CreateRelationalDataReader()
        => new();

    /// <summary>
    ///     Populates this command from the provided <paramref name="commandTemplate" />.
    /// </summary>
    /// <param name="commandTemplate">A template command from which the command text and parameters will be copied.</param>
    public virtual void PopulateFrom(IRelationalCommandTemplate commandTemplate)
    {
        CommandText = commandTemplate.CommandText;
        Parameters = commandTemplate.Parameters;
    }
}
