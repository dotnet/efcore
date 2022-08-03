// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerModificationCommandBatch : AffectedCountModificationCommandBatch
{
    private const int DefaultNetworkPacketSizeBytes = 4096;
    private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;
    private const int MaxParameterCount = 2100;
    private readonly List<IReadOnlyModificationCommand> _pendingBulkInsertCommands = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int maxBatchSize)
        : base(dependencies)
        => MaxBatchSize = maxBatchSize;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected new virtual ISqlServerUpdateSqlGenerator UpdateSqlGenerator
        => (ISqlServerUpdateSqlGenerator)base.UpdateSqlGenerator;

    /// <summary>
    ///     The maximum number of <see cref="ModificationCommand"/> instances that can be added to a single batch.
    /// </summary>
    /// <remarks>
    ///     For SQL Server, this is 42 by default, and cannot exceed 1000.
    /// </remarks>
    protected override int MaxBatchSize { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void RollbackLastCommand()
    {
        if (_pendingBulkInsertCommands.Count > 0)
        {
            _pendingBulkInsertCommands.RemoveAt(_pendingBulkInsertCommands.Count - 1);
            return;
        }

        base.RollbackLastCommand();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValid()
        => SqlBuilder.Length < MaxScriptLength
            // A single implicit parameter for the command text itself
            && ParameterValues.Count + 1 < MaxParameterCount;

    private void ApplyPendingBulkInsertCommands()
    {
        if (_pendingBulkInsertCommands.Count == 0)
        {
            return;
        }

        var commandPosition = CommandResultSet.Count;

        var wasCachedCommandTextEmpty = IsCommandTextEmpty;

        var resultSetMapping = UpdateSqlGenerator.AppendBulkInsertOperation(
            SqlBuilder, _pendingBulkInsertCommands, commandPosition, out var resultsContainPositionMapping,
            out var requiresTransaction);

        SetRequiresTransaction(!wasCachedCommandTextEmpty || requiresTransaction);

        if (resultsContainPositionMapping)
        {
            if (ResultsPositionalMappingEnabled is null)
            {
                ResultsPositionalMappingEnabled = new BitArray(CommandResultSet.Count + _pendingBulkInsertCommands.Count);
            }
            else
            {
                ResultsPositionalMappingEnabled.Length = CommandResultSet.Count + _pendingBulkInsertCommands.Count;
            }

            for (var i = commandPosition; i < commandPosition + _pendingBulkInsertCommands.Count; i++)
            {
                ResultsPositionalMappingEnabled![i] = true;
            }
        }

        foreach (var pendingCommand in _pendingBulkInsertCommands)
        {
            AddParameters(pendingCommand);

            CommandResultSet.Add(resultSetMapping);
        }

        if (resultSetMapping != ResultSetMapping.NoResultSet)
        {
            CommandResultSet[^1] = ResultSetMapping.LastInResultSet;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (modificationCommand.EntityState == EntityState.Added)
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Complete(bool moreBatchesExpected)
    {
        ApplyPendingBulkInsertCommands();

        base.Complete(moreBatchesExpected);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Execute(IRelationalConnection connection)
    {
        try
        {
            base.Execute(connection);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 334 } )
        {
            // SQL Server error: The target table '%.*ls' of the DML statement cannot have any enabled triggers if the statement contains an
            // OUTPUT clause without INTO clause.
            // This occurs when the user hasn't declared in metadata that a table has triggers, but triggers do exist in the database.
            // Throw a specialized exception to point the user in the right direction.
            throw new DbUpdateException(
                SqlServerStrings.SaveChangesFailedBecauseOfTriggers,
                e.InnerException,
                e.Entries);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 4186 } )
        {
            // SQL Server error: Column '%ls.%.*ls' cannot be referenced in the OUTPUT clause because the column definition contains a
            // subquery or references a function that performs user or system data access [...]
            // See https://docs.microsoft.com/sql/relational-databases/errors-events/mssqlserver-4186-database-engine-error
            throw new DbUpdateException(
                SqlServerStrings.SaveChangesFailedBecauseOfComputedColumnWithFunction,
                e.InnerException,
                e.Entries);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task ExecuteAsync(
        IRelationalConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await base.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 334 } )
        {
            // SQL Server error: The target table '%.*ls' of the DML statement cannot have any enabled triggers if the statement contains an
            // OUTPUT clause without INTO clause.
            // This occurs when the user hasn't declared in metadata that a table has triggers, but triggers do exist in the database.
            // Throw a specialized exception to point the user in the right direction.
            throw new DbUpdateException(
                SqlServerStrings.SaveChangesFailedBecauseOfTriggers,
                e.InnerException,
                e.Entries);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 4186 } )
        {
            // SQL Server error: Column '%ls.%.*ls' cannot be referenced in the OUTPUT clause because the column definition contains a
            // subquery or references a function that performs user or system data access [...]
            // See https://docs.microsoft.com/sql/relational-databases/errors-events/mssqlserver-4186-database-engine-error
            throw new DbUpdateException(
                SqlServerStrings.SaveChangesFailedBecauseOfComputedColumnWithFunction,
                e.InnerException,
                e.Entries);
        }
    }
}
