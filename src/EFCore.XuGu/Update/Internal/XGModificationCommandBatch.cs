// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class XGModificationCommandBatch : AffectedCountModificationCommandBatch
{
    private readonly List<IReadOnlyModificationCommand> _pendingBulkInsertCommands = new();

    public XGModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int maxBatchSize)
        : base(dependencies, maxBatchSize)
    {
    }

    protected new virtual IXGUpdateSqlGenerator UpdateSqlGenerator
        => (IXGUpdateSqlGenerator)base.UpdateSqlGenerator;

    protected override void RollbackLastCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (_pendingBulkInsertCommands.Count > 0)
        {
            _pendingBulkInsertCommands.RemoveAt(_pendingBulkInsertCommands.Count - 1);
        }

        //////
        // Pulled up from the base implementation to support our _pendingParameters field:

        for (var i = 0; i < _pendingParameters; i++)
        {
            var parameterIndex = RelationalCommandBuilder.Parameters.Count - 1;
            var parameter = RelationalCommandBuilder.Parameters[parameterIndex];

            RelationalCommandBuilder.RemoveParameterAt(parameterIndex);
            ParameterValues.Remove(parameter.InvariantName);
        }

        //
        //////

        base.RollbackLastCommand(modificationCommand);
    }

    private void ApplyPendingBulkInsertCommands()
    {
        if (_pendingBulkInsertCommands.Count == 0)
        {
            return;
        }

        var commandPosition = ResultSetMappings.Count;

        var wasCachedCommandTextEmpty = IsCommandTextEmpty;

        var resultSetMapping = UpdateSqlGenerator.AppendBulkInsertOperation(
            SqlBuilder, _pendingBulkInsertCommands, commandPosition, out var requiresTransaction);

        SetRequiresTransaction(!wasCachedCommandTextEmpty || requiresTransaction);

        for (var i = 0; i < _pendingBulkInsertCommands.Count; i++)
        {
            ResultSetMappings.Add(resultSetMapping);
        }

        if (resultSetMapping != ResultSetMapping.NoResults)
        {
            ResultSetMappings[^1] = ResultSetMapping.LastInResultSet;
        }
    }

    protected override void AddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (modificationCommand.EntityState == EntityState.Added && modificationCommand.StoreStoredProcedure is null)
        {
            if (_pendingBulkInsertCommands.Count > 0
                && !CanBeInsertedInSameStatement(_pendingBulkInsertCommands[0], modificationCommand))
            {
                // The new Add command cannot be added to the pending bulk insert commands (e.g. different table).
                // Write out the pending commands before starting a new pending chain.
                ApplyPendingBulkInsertCommands();
                _pendingBulkInsertCommands.Clear();
            }

            _pendingBulkInsertCommands.Add(modificationCommand);
            AddParameters(modificationCommand);
        }
        else
        {
            // If we have any pending bulk insert commands, write them out before the next non-Add command
            if (_pendingBulkInsertCommands.Count > 0)
            {
                // Note that we don't care about the transactionality of the bulk insert SQL, since there's the additional non-Add
                // command coming right afterwards, and so a transaction is required in any case.
                ApplyPendingBulkInsertCommands();
                _pendingBulkInsertCommands.Clear();
            }

            base.AddCommand(modificationCommand);
        }
    }

    private static bool CanBeInsertedInSameStatement(
        IReadOnlyModificationCommand firstCommand,
        IReadOnlyModificationCommand secondCommand)
        => firstCommand.TableName == secondCommand.TableName
            && firstCommand.Schema == secondCommand.Schema
            && firstCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName).SequenceEqual(
                secondCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName))
            && firstCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName).SequenceEqual(
                secondCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName));

    public override void Complete(bool moreBatchesExpected)
    {
        ApplyPendingBulkInsertCommands();

        base.Complete(moreBatchesExpected);
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="ReaderModificationCommandBatch.Execute" />,
    ///     propagating values back into the <see cref="ModificationCommand" />.
    /// </summary>
    /// <param name="startCommandIndex">The ordinal of the first command being consumed.</param>
    /// <param name="reader">The data reader.</param>
    /// <returns>The ordinal of the next result set that must be consumed.</returns>
    protected override int ConsumeResultSet(int startCommandIndex, RelationalDataReader reader)
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

                var command = ModificationCommands[
                    resultSetMapping.HasFlag(ResultSetMapping.IsPositionalResultMappingEnabled)
                        ? startCommandIndex + reader.DbDataReader.GetInt32(reader.DbDataReader.FieldCount - 1)
                        : commandIndex];

                Check.DebugAssert(
                    !resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly),
                    "!resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)");

                //////
                // Addition to base method:
                ConsumeRowsAffectedFromResultSet(command, reader, commandIndex);
                //
                //////

                command.PropagateResults(reader);
            }

            rowsAffected++;
        }
        while (++commandIndex < ResultSetMappings.Count
               && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet));

        return commandIndex - 1;
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
    protected override async Task<int> ConsumeResultSetAsync(int startCommandIndex, RelationalDataReader reader, CancellationToken cancellationToken)
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

                var command = ModificationCommands[
                    resultSetMapping.HasFlag(ResultSetMapping.IsPositionalResultMappingEnabled)
                        ? startCommandIndex + reader.DbDataReader.GetInt32(reader.DbDataReader.FieldCount - 1)
                        : commandIndex];

                Check.DebugAssert(
                    !resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly),
                    "!resultSetMapping.HasFlag(ResultSetMapping.ResultSetWithRowsAffectedOnly)");

                //////
                // Addition to base method:
                ConsumeRowsAffectedFromResultSet(command, reader, commandIndex);
                //
                //////

                command.PropagateResults(reader);
            }

            rowsAffected++;
        }
        while (++commandIndex < ResultSetMappings.Count
               && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet));

        return commandIndex - 1;
    }

    protected virtual void ConsumeRowsAffectedFromResultSet(
        IReadOnlyModificationCommand command,
        RelationalDataReader reader,
        int commandIndex)
    {
        if (command.StoreStoredProcedure is not null &&
            command.RowsAffectedColumn is { } rowsAffectedColumn)
        {
            var rowsAffectedParameter = (IStoreStoredProcedureParameter)rowsAffectedColumn;

            Debug.Assert(rowsAffectedParameter.Direction == ParameterDirection.Output);

            var readerIndex = -1;

            for (var i = 0; i < command.ColumnModifications.Count; i++)
            {
                var columnModification = command.ColumnModifications[i];
                if (columnModification.Column is IStoreStoredProcedureParameter
                    {
                        Direction: ParameterDirection.Output or ParameterDirection.InputOutput
                    })
                {
                    readerIndex++;
                }

                if (columnModification.Column == rowsAffectedColumn)
                {
                    break;
                }
            }

            if (reader.DbDataReader.GetInt32(readerIndex) != 1)
            {
                ThrowAggregateUpdateConcurrencyException(reader, commandIndex + 1, 1, 0);
            }
        }
    }

    protected override void AddParameter(IColumnModification columnModification)
    {
        var direction = columnModification.Column switch
        {
            IStoreStoredProcedureParameter storedProcedureParameter => storedProcedureParameter.Direction,
            IStoreStoredProcedureReturnValue => ParameterDirection.Output,
            _ => ParameterDirection.Input
        };

        //////
        // Start of injected code.

        // MySQL stored procedures cannot return a regular result set, and output parameter values are simply sent back as the
        // result set; this is very different from SQL Server, where output parameter values can be sent back in addition to result
        // sets. So we avoid adding XGParameters for output parameters - we'll just retrieve and propagate the values below when
        // consuming the result set.
        // Because XuguClient throws if we use an INOUT or OUT parameter for CommandType.Text commands, we skip
        // ParameterDirection.Output parameters entirely and change ParameterDirection.InputOutput to ParameterDirection.Input.
        if (columnModification.Column is IStoreStoredProcedureParameter parameter)
        {
            if (parameter.Direction.HasFlag(ParameterDirection.Output))
            {
                if (!parameter.Direction.HasFlag(ParameterDirection.Input))
                {
                    return;
                }

                direction = ParameterDirection.Input;

                var value = columnModification.UseCurrentValueParameter
                    ? columnModification.Value
                    : columnModification.UseOriginalValueParameter
                        ? columnModification.OriginalValue
                        : null;

                if (value is null)
                {
                    return;
                }
            }
        }

        // End of injected code.
        //////

        // For the case where the same modification has both current and original value parameters, and corresponds to an in/out parameter,
        // we only want to add a single parameter. This will happen below.
        if (columnModification.UseCurrentValueParameter
            && !(columnModification.UseOriginalValueParameter && direction == ParameterDirection.InputOutput))
        {
            AddParameterCore(
                columnModification.ParameterName, columnModification.UseCurrentValue
                    ? columnModification.Value
                    : direction == ParameterDirection.InputOutput
                        ? DBNull.Value
                        : null);
        }

        if (columnModification.UseOriginalValueParameter)
        {
            Check.DebugAssert(direction.HasFlag(ParameterDirection.Input), "direction.HasFlag(ParameterDirection.Input)");

            AddParameterCore(columnModification.OriginalParameterName, columnModification.OriginalValue);
        }

        void AddParameterCore(string name, object value)
        {
            RelationalCommandBuilder.AddParameter(
                name,
                Dependencies.SqlGenerationHelper.GenerateParameterName(name),
                columnModification.TypeMapping!,
                columnModification.IsNullable,
                direction);

            ParameterValues.Add(name, value);

            _pendingParameters++;
        }
    }

    /// <summary>
    /// We override this method only to support our _pendingParameters field.
    /// </summary>
    public override bool TryAddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (StoreCommand is not null)
        {
            throw new InvalidOperationException(RelationalStrings.ModificationCommandBatchAlreadyComplete);
        }

        if (ModificationCommands.Count >= MaxBatchSize)
        {
            return false;
        }

        _pendingParameters = 0;

        return base.TryAddCommand(modificationCommand);
    }

    /// <summary>
    /// We use _pendingParameters only to support our AddParameter implementation.
    /// </summary>
    private int _pendingParameters;
}
