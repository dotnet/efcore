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
    protected AffectedCountModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.Execute" />.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    protected override void Consume(RelationalDataReader reader)
    {
        Check.DebugAssert(
            CommandResultSet.Count == ModificationCommands.Count,
            $"CommandResultSet.Count of {CommandResultSet.Count} != ModificationCommands.Count of {ModificationCommands.Count}");

        var commandIndex = 0;

        try
        {
            var actualResultSetCount = 0;
            do
            {
                while (commandIndex < CommandResultSet.Count
                       && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                {
                    commandIndex++;
                }

                if (commandIndex < CommandResultSet.Count)
                {
                    commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                        ? ConsumeResultSetWithPropagation(commandIndex, reader)
                        : ConsumeResultSetWithoutPropagation(commandIndex, reader);
                    actualResultSetCount++;
                }
            }
            while (commandIndex < CommandResultSet.Count
                   && reader.DbDataReader.NextResult());

#if DEBUG
            while (commandIndex < CommandResultSet.Count
                   && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
            {
                commandIndex++;
            }

            Check.DebugAssert(
                commandIndex == ModificationCommands.Count,
                "Expected " + ModificationCommands.Count + " results, got " + commandIndex);

            var expectedResultSetCount = CommandResultSet.Count(e => e == ResultSetMapping.LastInResultSet);

            Check.DebugAssert(
                actualResultSetCount == expectedResultSetCount,
                "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
#endif
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex].Entries);
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
            CommandResultSet.Count == ModificationCommands.Count,
            $"CommandResultSet.Count of {CommandResultSet.Count} != ModificationCommands.Count of {ModificationCommands.Count}");

        var commandIndex = 0;

        try
        {
            var actualResultSetCount = 0;
            do
            {
                while (commandIndex < CommandResultSet.Count
                       && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                {
                    commandIndex++;
                }

                if (commandIndex < CommandResultSet.Count)
                {
                    commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                        ? await ConsumeResultSetWithPropagationAsync(commandIndex, reader, cancellationToken).ConfigureAwait(false)
                        : await ConsumeResultSetWithoutPropagationAsync(commandIndex, reader, cancellationToken).ConfigureAwait(false);
                    actualResultSetCount++;
                }
            }
            while (commandIndex < CommandResultSet.Count
                   && await reader.DbDataReader.NextResultAsync(cancellationToken).ConfigureAwait(false));

#if DEBUG
            while (commandIndex < CommandResultSet.Count
                   && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
            {
                commandIndex++;
            }

            Check.DebugAssert(
                commandIndex == ModificationCommands.Count,
                "Expected " + ModificationCommands.Count + " results, got " + commandIndex);

            var expectedResultSetCount = CommandResultSet.Count(e => e == ResultSetMapping.LastInResultSet);

            Check.DebugAssert(
                actualResultSetCount == expectedResultSetCount,
                "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
#endif
        }
        catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
        {
            throw new DbUpdateException(
                RelationalStrings.UpdateStoreException,
                ex,
                ModificationCommands[commandIndex].Entries);
        }
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.Execute" />,
    ///     propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="startResultSetIndex">The ordinal of the first result set being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <returns>The ordinal of the next result set that must be consumed.</returns>
    protected virtual int ConsumeResultSetWithPropagation(int startResultSetIndex, RelationalDataReader reader)
    {
        var resultSetIndex = startResultSetIndex;
        var rowsAffected = 0;
        do
        {
            if (!reader.Read())
            {
                var expectedRowsAffected = rowsAffected + 1;
                while (++resultSetIndex < CommandResultSet.Count
                       && CommandResultSet[resultSetIndex - 1] == ResultSetMapping.NotLastInResultSet)
                {
                    expectedRowsAffected++;
                }

                ThrowAggregateUpdateConcurrencyException(resultSetIndex, expectedRowsAffected, rowsAffected);
            }

            var tableModification = ModificationCommands[
                ResultsPositionalMappingEnabled?.Length > resultSetIndex && ResultsPositionalMappingEnabled[resultSetIndex]
                    ? startResultSetIndex + reader.DbDataReader.GetInt32(reader.DbDataReader.FieldCount - 1)
                    : resultSetIndex];

            Check.DebugAssert(tableModification.RequiresResultPropagation, "RequiresResultPropagation is false");

            var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);

            tableModification.PropagateResults(valueBufferFactory.Create(reader.DbDataReader));
            rowsAffected++;
        }
        while (++resultSetIndex < CommandResultSet.Count
               && CommandResultSet[resultSetIndex - 1] == ResultSetMapping.NotLastInResultSet);

        return resultSetIndex;
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.ExecuteAsync" />,
    ///     propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="startResultSetIndex">The ordinal of the first result set being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task contains the ordinal of the next command that must be consumed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual async Task<int> ConsumeResultSetWithPropagationAsync(
        int startResultSetIndex,
        RelationalDataReader reader,
        CancellationToken cancellationToken)
    {
        var resultSetIndex = startResultSetIndex;
        var rowsAffected = 0;
        do
        {
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var expectedRowsAffected = rowsAffected + 1;
                while (++resultSetIndex < CommandResultSet.Count
                       && CommandResultSet[resultSetIndex - 1] == ResultSetMapping.NotLastInResultSet)
                {
                    expectedRowsAffected++;
                }

                ThrowAggregateUpdateConcurrencyException(resultSetIndex, expectedRowsAffected, rowsAffected);
            }

            var tableModification = ModificationCommands[
                ResultsPositionalMappingEnabled?.Length > resultSetIndex && ResultsPositionalMappingEnabled[resultSetIndex]
                    ? startResultSetIndex + reader.DbDataReader.GetInt32(reader.DbDataReader.FieldCount - 1)
                    : resultSetIndex];

            Check.DebugAssert(tableModification.RequiresResultPropagation, "RequiresResultPropagation is false");

            var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);

            tableModification.PropagateResults(valueBufferFactory.Create(reader.DbDataReader));
            rowsAffected++;
        }
        while (++resultSetIndex < CommandResultSet.Count
               && CommandResultSet[resultSetIndex - 1] == ResultSetMapping.NotLastInResultSet);

        return resultSetIndex;
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.Execute" />
    ///     without propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="commandIndex">The ordinal of the command being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <returns>The ordinal of the next command that must be consumed.</returns>
    protected virtual int ConsumeResultSetWithoutPropagation(int commandIndex, RelationalDataReader reader)
    {
        var expectedRowsAffected = 1;
        while (++commandIndex < CommandResultSet.Count
               && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
        {
            Check.DebugAssert(!ModificationCommands[commandIndex].RequiresResultPropagation, "RequiresResultPropagation is true");

            expectedRowsAffected++;
        }

        if (reader.Read())
        {
            var rowsAffected = reader.DbDataReader.GetInt32(0);
            if (rowsAffected != expectedRowsAffected)
            {
                ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
            }
        }
        else
        {
            ThrowAggregateUpdateConcurrencyException(commandIndex, 1, 0);
        }

        return commandIndex;
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
    protected virtual async Task<int> ConsumeResultSetWithoutPropagationAsync(
        int commandIndex,
        RelationalDataReader reader,
        CancellationToken cancellationToken)
    {
        var expectedRowsAffected = 1;
        while (++commandIndex < CommandResultSet.Count
               && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
        {
            Check.DebugAssert(!ModificationCommands[commandIndex].RequiresResultPropagation, "RequiresResultPropagation is true");

            expectedRowsAffected++;
        }

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var rowsAffected = reader.DbDataReader.GetInt32(0);
            if (rowsAffected != expectedRowsAffected)
            {
                ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
            }
        }
        else
        {
            ThrowAggregateUpdateConcurrencyException(commandIndex, 1, 0);
        }

        return commandIndex;
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
    /// <param name="commandIndex">The ordinal of the command.</param>
    /// <param name="expectedRowsAffected">The expected number of rows affected.</param>
    /// <param name="rowsAffected">The actual number of rows affected.</param>
    protected virtual void ThrowAggregateUpdateConcurrencyException(
        int commandIndex,
        int expectedRowsAffected,
        int rowsAffected)
        => throw new DbUpdateConcurrencyException(
            RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
            AggregateEntries(commandIndex, expectedRowsAffected));
}
