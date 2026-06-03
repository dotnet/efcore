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

    private readonly List<IReadOnlyModificationCommand> _pendingBulkInsertCommands = [];

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

        var commandPosition = ResultSetMappings.Count;

        var wasCachedCommandTextEmpty = IsCommandTextEmpty;

        var resultSetMapping = UpdateSqlGenerator.AppendBulkInsertOperation(
            SqlBuilder, _pendingBulkInsertCommands, commandPosition, out var requiresTransaction);

        SetRequiresTransaction(!wasCachedCommandTextEmpty || requiresTransaction);

        for (var i = 0; i < _pendingBulkInsertCommands.Count; i++)
        {
            ResultSetMappings.Add(resultSetMapping);
        }

        // All result mappings are marked as "not last", mark the last one as "last".
        if (resultSetMapping.HasFlag(ResultSetMapping.HasResultRow))
        {
            ResultSetMappings[^1] &= ~ResultSetMapping.NotLastInResultSet;
            ResultSetMappings[^1] |= ResultSetMapping.LastInResultSet;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool TryAddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        // If there are any pending bulk insert commands and the new command is incompatible with them (not an insert, insert into a
        // separate table..), apply the pending commands.
        if (_pendingBulkInsertCommands.Count > 0
            && (modificationCommand.EntityState != EntityState.Added
                || modificationCommand.StoreStoredProcedure is not null
                || !CanBeInsertedInSameStatement(_pendingBulkInsertCommands[0], modificationCommand)))
        {
            ApplyPendingBulkInsertCommands();
            _pendingBulkInsertCommands.Clear();
        }

        return base.TryAddCommand(modificationCommand);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        // TryAddCommand above already applied any pending commands if the new command is incompatible with them.
        // So if the new command is an insert, just append it to pending, otherwise do the regular add logic.
        if (modificationCommand is { EntityState: EntityState.Added, StoreStoredProcedure: null })
        {
            _pendingBulkInsertCommands.Add(modificationCommand);
            AddParameters(modificationCommand);
        }
        else
        {
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
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 334 })
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
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 4186 })
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
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 334 })
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
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 4186 })
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
