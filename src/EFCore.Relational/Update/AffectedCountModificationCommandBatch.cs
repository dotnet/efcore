// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         A <see cref="ReaderModificationCommandBatch" /> for providers which return values to find out how many rows were affected.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public abstract class AffectedCountModificationCommandBatch : ReaderModificationCommandBatch
{
    /// <summary>
    ///     Creates a new <see cref="AffectedCountModificationCommandBatch" /> instance.
    /// </summary>
    /// <param name="dependencies">Service dependencies.</param>
    /// <param name="maxBatchSize">The maximum batch size. Defaults to 1000.</param>
    protected AffectedCountModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies, int? maxBatchSize = null)
        : base(dependencies, maxBatchSize)
    {
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.Execute" />.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    protected override void Consume(RelationalDataReader reader)
    {
        Check.DebugAssert(
            ResultSetMappings.Count == ModificationCommands.Count,
            $"CommandResultSet.Count of {ResultSetMappings.Count} != ModificationCommands.Count of {ModificationCommands.Count}");

        var commandIndex = 0;

        try
        {
            bool? onResultSet = null;
            var hasOutputParameters = false;

            while (commandIndex < ResultSetMappings.Count)
            {
                var resultSetMapping = ResultSetMappings[commandIndex];

                if (resultSetMapping.HasFlag(ResultSetMapping.HasResultRow))
                {
                    if (onResultSet == false)
                    {
                        throw new InvalidOperationException(RelationalStrings.MissingResultSetWhenSaving);
                    }

                    var lastHandledCommandIndex = resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)
                        ? ConsumeResultSetWithRowsAffectedOnly(commandIndex, reader)
                        : ConsumeResultSet(commandIndex, reader);

                    Check.DebugAssert(
                        resultSetMapping.HasFlag(ResultSetMapping.LastInResultSet)
                            ? lastHandledCommandIndex == commandIndex
                            : lastHandledCommandIndex > commandIndex, "Bad handling of ResultSetMapping and command indexing");

                    commandIndex = lastHandledCommandIndex + 1;

                    onResultSet = reader.DbDataReader.NextResult();
                }
                else
                {
                    commandIndex++;
                }

                if (resultSetMapping.HasFlag(ResultSetMapping.HasOutputParameters))
                {
                    hasOutputParameters = true;
                }
            }

            if (onResultSet == true)
            {
                Dependencies.UpdateLogger.UnexpectedTrailingResultSetWhenSaving();
            }

            reader.Close();

            if (hasOutputParameters)
            {
                var parameterCounter = 0;
                IReadOnlyModificationCommand command;

                for (commandIndex = 0; commandIndex < ResultSetMappings.Count; commandIndex++, parameterCounter += ParameterCount(command))
                {
                    command = ModificationCommands[commandIndex];

                    Check.DebugAssert(
                        command.ColumnModifications.All(c => c.UseParameter),
                        "This code assumes all column modifications involve a DbParameter (see counting above)");

                    if (!ResultSetMappings[commandIndex].HasFlag(ResultSetMapping.HasOutputParameters))
                    {
                        continue;
                    }

                    // Note: we assume that the return value is the parameter at position 0, and skip it here for the purpose of calculating
                    // the right baseParameterIndex to pass to PropagateOutputParameters below.
                    var rowsAffectedDbParameter = command.RowsAffectedColumn is IStoreStoredProcedureParameter rowsAffectedParameter
                        ? reader.DbCommand.Parameters[parameterCounter + rowsAffectedParameter.Position]
                        : command.StoreStoredProcedure!.ReturnValue is not null
                            ? reader.DbCommand.Parameters[parameterCounter++]
                            : null;

                    if (rowsAffectedDbParameter is not null)
                    {
                        if (rowsAffectedDbParameter.Value is int rowsAffected)
                        {
                            if (rowsAffected != 1)
                            {
                                ThrowAggregateUpdateConcurrencyException(
                                    reader, commandIndex + 1, expectedRowsAffected: 1, rowsAffected: 0);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.StoredProcedureRowsAffectedNotPopulated(
                                    command.StoreStoredProcedure!.SchemaQualifiedName));
                        }
                    }

                    command.PropagateOutputParameters(reader.DbCommand.Parameters, parameterCounter);
                }
            }
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex < ModificationCommands.Count ? commandIndex : ModificationCommands.Count - 1].Entries);
        }
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.ExecuteAsync" />.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected override async Task ConsumeAsync(
        RelationalDataReader reader,
        CancellationToken cancellationToken = default)
    {
        Check.DebugAssert(
            ResultSetMappings.Count == ModificationCommands.Count,
            $"CommandResultSet.Count of {ResultSetMappings.Count} != ModificationCommands.Count of {ModificationCommands.Count}");

        var commandIndex = 0;

        try
        {
            bool? onResultSet = null;
            var hasOutputParameters = false;

            while (commandIndex < ResultSetMappings.Count)
            {
                var resultSetMapping = ResultSetMappings[commandIndex];

                if (resultSetMapping.HasFlag(ResultSetMapping.HasResultRow))
                {
                    if (onResultSet == false)
                    {
                        throw new InvalidOperationException(RelationalStrings.MissingResultSetWhenSaving);
                    }

                    var lastHandledCommandIndex = resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)
                        ? await ConsumeResultSetWithRowsAffectedOnlyAsync(commandIndex, reader, cancellationToken).ConfigureAwait(false)
                        : await ConsumeResultSetAsync(commandIndex, reader, cancellationToken).ConfigureAwait(false);

                    Check.DebugAssert(
                        resultSetMapping.HasFlag(ResultSetMapping.LastInResultSet)
                            ? lastHandledCommandIndex == commandIndex
                            : lastHandledCommandIndex > commandIndex, "Bad handling of ResultSetMapping and command indexing");

                    commandIndex = lastHandledCommandIndex + 1;

                    onResultSet = await reader.DbDataReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    commandIndex++;
                }

                if (resultSetMapping.HasFlag(ResultSetMapping.HasOutputParameters))
                {
                    hasOutputParameters = true;
                }
            }

            if (onResultSet == true)
            {
                Dependencies.UpdateLogger.UnexpectedTrailingResultSetWhenSaving();
            }

            await reader.CloseAsync().ConfigureAwait(false);

            if (hasOutputParameters)
            {
                var parameterCounter = 0;
                IReadOnlyModificationCommand command;

                for (commandIndex = 0; commandIndex < ResultSetMappings.Count; commandIndex++, parameterCounter += ParameterCount(command))
                {
                    command = ModificationCommands[commandIndex];

                    Check.DebugAssert(
                        command.ColumnModifications.All(c => c.UseParameter),
                        "This code assumes all column modifications involve a DbParameter (see counting above)");

                    if (!ResultSetMappings[commandIndex].HasFlag(ResultSetMapping.HasOutputParameters))
                    {
                        continue;
                    }

                    // Note: we assume that the return value is the parameter at position 0, and skip it here for the purpose of calculating
                    // the right baseParameterIndex to pass to PropagateOutputParameters below.
                    var rowsAffectedDbParameter = command.RowsAffectedColumn is IStoreStoredProcedureParameter rowsAffectedParameter
                        ? reader.DbCommand.Parameters[parameterCounter + rowsAffectedParameter.Position]
                        : command.StoreStoredProcedure!.ReturnValue is not null
                            ? reader.DbCommand.Parameters[parameterCounter++]
                            : null;

                    if (rowsAffectedDbParameter is not null)
                    {
                        if (rowsAffectedDbParameter.Value is int rowsAffected)
                        {
                            if (rowsAffected != 1)
                            {
                                await ThrowAggregateUpdateConcurrencyExceptionAsync(
                                        reader, commandIndex + 1, expectedRowsAffected: 1, rowsAffected: 0, cancellationToken)
                                    .ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.StoredProcedureRowsAffectedNotPopulated(
                                    command.StoreStoredProcedure!.SchemaQualifiedName));
                        }
                    }

                    command.PropagateOutputParameters(reader.DbCommand.Parameters, parameterCounter);
                }
            }
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex < ModificationCommands.Count ? commandIndex : ModificationCommands.Count - 1].Entries);
        }
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.Execute" />,
    ///     propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="startCommandIndex">The ordinal of the first command being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <returns>The ordinal of the next result set that must be consumed.</returns>
    protected virtual int ConsumeResultSet(int startCommandIndex, RelationalDataReader reader)
    {
        IReadOnlyModificationCommand? command = null;

        try
        {
            var commandIndex = startCommandIndex;
            var rowsAffected = 0;
            do
            {
                if (!reader.Read())
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while (++commandIndex < ResultSetMappings.Count
                           && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet))
                    {
                        expectedRowsAffected++;
                    }

                    ThrowAggregateUpdateConcurrencyException(reader, commandIndex, expectedRowsAffected, rowsAffected);
                }
                else
                {
                    var resultSetMapping = ResultSetMappings[commandIndex];

                    command = ModificationCommands[
                        resultSetMapping.HasFlag(ResultSetMapping.IsPositionalResultMappingEnabled)
                            ? startCommandIndex + reader.DbDataReader.GetInt32(reader.DbDataReader.FieldCount - 1)
                            : commandIndex];

                    Check.DebugAssert(
                        !resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly),
                        "!resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)");

                    command.PropagateResults(reader);

                    command = null;
                }

                rowsAffected++;
            }
            while (++commandIndex < ResultSetMappings.Count
                   && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet));

            return commandIndex - 1;
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                command?.Entries ?? ModificationCommands.SelectMany(c => c.Entries).ToList());
        }
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.ExecuteAsync" />,
    ///     propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="startCommandIndex">The ordinal of the first result set being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task contains the ordinal of the next command that must be consumed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual async Task<int> ConsumeResultSetAsync(
        int startCommandIndex,
        RelationalDataReader reader,
        CancellationToken cancellationToken)
    {
        IReadOnlyModificationCommand? command = null;

        try
        {
            var commandIndex = startCommandIndex;
            var rowsAffected = 0;
            do
            {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while (++commandIndex < ResultSetMappings.Count
                           && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet))
                    {
                        expectedRowsAffected++;
                    }

                    await ThrowAggregateUpdateConcurrencyExceptionAsync(
                        reader, commandIndex, expectedRowsAffected, rowsAffected, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var resultSetMapping = ResultSetMappings[commandIndex];

                    command = ModificationCommands[
                        resultSetMapping.HasFlag(ResultSetMapping.IsPositionalResultMappingEnabled)
                            ? startCommandIndex + reader.DbDataReader.GetInt32(reader.DbDataReader.FieldCount - 1)
                            : commandIndex];

                    Check.DebugAssert(
                        !resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly),
                        "!resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)");

                    command.PropagateResults(reader);

                    command = null;
                }

                rowsAffected++;
            }
            while (++commandIndex < ResultSetMappings.Count
                   && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet));

            return commandIndex - 1;
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                command?.Entries ?? ModificationCommands.SelectMany(c => c.Entries).ToList());
        }
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.Execute" />
    ///     without propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="commandIndex">The ordinal of the command being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <returns>The ordinal of the next command that must be consumed.</returns>
    protected virtual int ConsumeResultSetWithRowsAffectedOnly(int commandIndex, RelationalDataReader reader)
    {
        var expectedRowsAffected = 1;
        while (++commandIndex < ResultSetMappings.Count
               && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet))
        {
            Check.DebugAssert(
                ResultSetMappings[commandIndex].HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly),
                "ResultSetMappings[commandIndex].HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)");

            expectedRowsAffected++;
        }

        if (reader.Read())
        {
            var rowsAffected = reader.DbDataReader.GetInt32(0);
            if (rowsAffected != expectedRowsAffected)
            {
                ThrowAggregateUpdateConcurrencyException(reader, commandIndex, expectedRowsAffected, rowsAffected);
            }
        }
        else
        {
            ThrowAggregateUpdateConcurrencyException(reader, commandIndex, 1, 0);
        }

        return commandIndex - 1;
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.ExecuteAsync" />
    ///     without propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="commandIndex">The ordinal of the command being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task contains the ordinal of the next command that must be consumed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual async Task<int> ConsumeResultSetWithRowsAffectedOnlyAsync(
        int commandIndex,
        RelationalDataReader reader,
        CancellationToken cancellationToken)
    {
        var expectedRowsAffected = 1;
        while (++commandIndex < ResultSetMappings.Count
               && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet))
        {
            Check.DebugAssert(
                ResultSetMappings[commandIndex].HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly),
                "ResultSetMappings[commandIndex].HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)");

            expectedRowsAffected++;
        }

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var rowsAffected = reader.DbDataReader.GetInt32(0);
            if (rowsAffected != expectedRowsAffected)
            {
                await ThrowAggregateUpdateConcurrencyExceptionAsync(
                    reader, commandIndex, expectedRowsAffected, rowsAffected, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            await ThrowAggregateUpdateConcurrencyExceptionAsync(
                reader, commandIndex, 1, 0, cancellationToken).ConfigureAwait(false);
        }

        return commandIndex - 1;
    }

    private static int ParameterCount(IReadOnlyModificationCommand command)
    {
        // As a shortcut, if the command uses a stored procedure, return the number of parameters directly from it.
        if (command.StoreStoredProcedure is { } storedProcedure)
        {
            return storedProcedure.Parameters.Count;
        }

        // Otherwise we need to count the total parameters used by all column modifications
        var parameterCount = 0;

        for (var i = 0; i < command.ColumnModifications.Count; i++)
        {
            var columnModification = command.ColumnModifications[i];

            if (columnModification.UseCurrentValueParameter)
            {
                parameterCount++;
            }

            if (columnModification.UseOriginalValueParameter)
            {
                parameterCount++;
            }
        }

        return parameterCount;
    }

    private IReadOnlyList<IUpdateEntry> AggregateEntries(int endIndex, int commandCount)
    {
        var entries = new List<IUpdateEntry>();
        for (var i = endIndex - commandCount; i < endIndex; i++)
        {
            entries.AddRange(ModificationCommands[i].Entries);
        }

        return entries;
    }

    /// <summary>
    ///     Throws an exception indicating the command affected an unexpected number of rows.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="commandIndex">The ordinal of the command.</param>
    /// <param name="expectedRowsAffected">The expected number of rows affected.</param>
    /// <param name="rowsAffected">The actual number of rows affected.</param>
    protected virtual void ThrowAggregateUpdateConcurrencyException(
        RelationalDataReader reader,
        int commandIndex,
        int expectedRowsAffected,
        int rowsAffected)
    {
        var entries = AggregateEntries(commandIndex, expectedRowsAffected);
        var exception = new DbUpdateConcurrencyException(
            RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
            entries);

        if (!Dependencies.UpdateLogger.OptimisticConcurrencyException(
                Dependencies.CurrentContext.Context,
                entries,
                exception,
                (c, ex, e, d) => CreateConcurrencyExceptionEventData(c, reader, ex, e, d)).IsSuppressed)
        {
            throw exception;
        }
    }

    /// <summary>
    ///     Throws an exception indicating the command affected an unexpected number of rows.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="commandIndex">The ordinal of the command.</param>
    /// <param name="expectedRowsAffected">The expected number of rows affected.</param>
    /// <param name="rowsAffected">The actual number of rows affected.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns> A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual async Task ThrowAggregateUpdateConcurrencyExceptionAsync(
        RelationalDataReader reader,
        int commandIndex,
        int expectedRowsAffected,
        int rowsAffected,
        CancellationToken cancellationToken)
    {
        var entries = AggregateEntries(commandIndex, expectedRowsAffected);
        var exception = new DbUpdateConcurrencyException(
            RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
            entries);

        if (!(await Dependencies.UpdateLogger.OptimisticConcurrencyExceptionAsync(
                    Dependencies.CurrentContext.Context,
                    entries,
                    exception,
                    (c, ex, e, d) => CreateConcurrencyExceptionEventData(c, reader, ex, e, d),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)).IsSuppressed)
        {
            throw exception;
        }
    }

    private static RelationalConcurrencyExceptionEventData CreateConcurrencyExceptionEventData(
        DbContext context,
        RelationalDataReader reader,
        DbUpdateConcurrencyException exception,
        IReadOnlyList<IUpdateEntry> entries,
        EventDefinition<Exception> definition)
        => new(
            definition,
            (definition1, payload)
                => ((EventDefinition<Exception>)definition1).GenerateMessage(((ConcurrencyExceptionEventData)payload).Exception),
            context,
            reader.RelationalConnection.DbConnection,
            reader.DbCommand,
            reader.DbDataReader,
            reader.CommandId,
            reader.RelationalConnection.ConnectionId,
            entries,
            exception);
}
