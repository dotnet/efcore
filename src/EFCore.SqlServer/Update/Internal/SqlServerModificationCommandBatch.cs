// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    // https://docs.microsoft.com/sql/sql-server/maximum-capacity-specifications-for-sql-server
    private const int DefaultNetworkPacketSizeBytes = 4096;
    private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;

    /// <summary>
    ///     The SQL Server limit on parameters, including two extra parameters to sp_executesql (@stmt and @params).
    /// </summary>
    private const int MaxParameterCount = 2100 - 2;

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
        : base(dependencies, maxBatchSize)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected new virtual ISqlServerUpdateSqlGenerator UpdateSqlGenerator
        => (ISqlServerUpdateSqlGenerator)base.UpdateSqlGenerator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void RollbackLastCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (_pendingBulkInsertCommands.Count > 0)
        {
            _pendingBulkInsertCommands.RemoveAt(_pendingBulkInsertCommands.Count - 1);
        }

        base.RollbackLastCommand(modificationCommand);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValid()
    {
        if (ParameterValues.Count > MaxParameterCount)
        {
            return false;
        }

        var sqlLength = SqlBuilder.Length;

        if (_pendingBulkInsertCommands.Count > 0)
        {
            // Conservative heuristic for the length of the pending bulk insert commands.
            // See EXEC sp_server_info.
            var numColumns = _pendingBulkInsertCommands[0].ColumnModifications.Count;

            sqlLength +=
                numColumns * 128 // column name lengths
                + 128 // schema name length
                + 128 // table name length
                + _pendingBulkInsertCommands.Count * numColumns * 6 // column parameter placeholders
                + 300; // some extra fixed overhead
        }

        return sqlLength < MaxScriptLength;
    }

    private void ApplyPendingBulkInsertCommands()
    {
        if (_pendingBulkInsertCommands.Count == 0)
        {
            return;
        }

        var commandPosition = CommandResultSet.Count;

        var wasCachedCommandTextEmpty = IsCommandTextEmpty;

        var resultSetMapping = UpdateSqlGenerator.AppendBulkInsertOperation(
            SqlBuilder, _pendingBulkInsertCommands, commandPosition, out var requiresTransaction);

        SetRequiresTransaction(!wasCachedCommandTextEmpty || requiresTransaction);

        for (var i = 0; i < _pendingBulkInsertCommands.Count; i++)
        {
            CommandResultSet.Add(resultSetMapping);
        }

        if (resultSetMapping != ResultSetMapping.NoResults)
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
