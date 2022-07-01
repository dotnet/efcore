// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerUpdateSqlGenerator : SelectingUpdateSqlGenerator, ISqlServerUpdateSqlGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerUpdateSqlGenerator(
        UpdateSqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     The minimum number of insertions which are executed using MERGE ... OUTPUT INTO. Below this threshold, multiple batched INSERT
    ///     statements are more efficient.
    /// </summary>
    protected virtual int MergeIntoMinimumThreshold => 4;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        // If no database-generated columns need to be read back, just do a simple INSERT (default behavior).
        // If there are generated columns but there are no triggers defined on the table, we can do a simple INSERT ... OUTPUT
        // (without INTO), which is also the default behavior, doesn't require a transaction and is the most efficient.
        if (command.ColumnModifications.All(o => !o.IsRead) || !HasAnyTriggers(command))
        {
            return AppendInsertReturningOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
        }

        // SQL Server doesn't allow INSERT ... OUTPUT on tables with triggers.
        // If the only generated column is an IDENTITY, do INSERT+SELECT which is relatively fast.
        // Otherwise fall back to INSERT ... OUTPUT INTO @inserted; SELECT ... FROM @inserted.
        var table = StoreObjectIdentifier.Table(command.TableName, command.Schema);

        return command.ColumnModifications.All(
            o =>
                !o.IsKey
                || !o.IsRead
                || o.Property?.GetValueGenerationStrategy(table) == SqlServerValueGenerationStrategy.IdentityColumn)
            ? AppendInsertAndSelectOperations(commandStringBuilder, command, commandPosition, out requiresTransaction)
            : AppendInsertSingleRowWithOutputInto(
                commandStringBuilder,
                command,
                command.ColumnModifications.Where(o => o.IsKey).ToList(),
                command.ColumnModifications.Where(o => o.IsRead).ToList(),
                commandPosition,
                out requiresTransaction);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AppendInsertCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> writeOperations,
        IReadOnlyList<IColumnModification> readOperations)
    {
        // In SQL Server the OUTPUT clause is placed differently (before the VALUES instead of at the end)
        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendOutputClause(commandStringBuilder, readOperations);
        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ResultSetMapping AppendUpdateOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        // We normally do a simple UPDATE with an OUTPUT clause (either for the generated columns, or for "1" for concurrency checking).
        // However, if there are triggers defined, OUTPUT (without INTO) is not supported, so we do UPDATE+SELECT.
        return HasAnyTriggers(command)
            ? AppendUpdateAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction)
            : AppendUpdateReturningOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AppendUpdateCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> writeOperations,
        IReadOnlyList<IColumnModification> readOperations,
        IReadOnlyList<IColumnModification> conditionOperations,
        string? additionalReadValues = null)
    {
        // In SQL Server the OUTPUT clause is placed differently (before the WHERE instead of at the end)
        AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendOutputClause(commandStringBuilder, readOperations, additionalReadValues);
        AppendWhereClause(commandStringBuilder, conditionOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ResultSetMapping AppendDeleteOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        // We normally do a simple DELETE, with an OUTPUT clause emitting "1" for concurrency checking.
        // However, if there are triggers defined, OUTPUT (without INTO) is not supported, so we do UPDATE+SELECT.
        return HasAnyTriggers(command)
            ? AppendDeleteAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction)
            : AppendDeleteReturningOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AppendDeleteCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> readOperations,
        IReadOnlyList<IColumnModification> conditionOperations,
        string? additionalReadValues = null)
    {
        // In SQL Server the OUTPUT clause is placed differently (before the WHERE instead of at the end)
        AppendDeleteCommandHeader(commandStringBuilder, name, schema);
        AppendOutputClause(commandStringBuilder, readOperations, additionalReadValues);
        AppendWhereClause(commandStringBuilder, conditionOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ResultSetMapping AppendBulkInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition,
        out bool resultsContainPositionMapping,
        out bool requiresTransaction)
    {
        resultsContainPositionMapping = false;

        var firstCommand = modificationCommands[0];

        if (modificationCommands.Count == 1)
        {
            return AppendInsertOperation(commandStringBuilder, firstCommand, commandPosition, out requiresTransaction);
        }

        var table = StoreObjectIdentifier.Table(firstCommand.TableName, modificationCommands[0].Schema);

        var readOperations = firstCommand.ColumnModifications.Where(o => o.IsRead).ToList();
        var writeOperations = firstCommand.ColumnModifications.Where(o => o.IsWrite).ToList();
        var keyOperations = firstCommand.ColumnModifications.Where(o => o.IsKey).ToList();

        var writableOperations = modificationCommands[0].ColumnModifications
            .Where(o =>
                o.Property?.GetValueGenerationStrategy(table) != SqlServerValueGenerationStrategy.IdentityColumn
                && o.Property?.GetComputedColumnSql() is null
                && o.Property?.GetColumnType() is not "rowversion" and not "timestamp")
            .ToList();

        if (writeOperations.Count == 0)
        {
            // We have no values to write; MERGE and multi-row INSERT cannot be used without writing at least a single column.
            // But as long as there's at least one writable column (non-identity/computed), we can use it to send DEFAULT in a multi-row
            // INSERT.
            if (writableOperations.Count > 0)
            {
                if (writableOperations.Count > 1)
                {
                    writableOperations.RemoveRange(1, writableOperations.Count - 1);
                }

                return readOperations.Count == 0
                    ? AppendInsertMultipleDefaultRows(
                        commandStringBuilder, modificationCommands, writableOperations, out requiresTransaction)
                    : AppendInsertMultipleDefaultRowsWithOutputInto(
                        commandStringBuilder, modificationCommands, commandPosition, writableOperations, keyOperations, readOperations,
                        out requiresTransaction);
            }

            // There are no writeable columns, fall back to sending multiple single-row INSERTs (there is no way to insert multiple
            // all-default rows in a single INSERT).
            requiresTransaction = modificationCommands.Count > 1;
            foreach (var modification in modificationCommands)
            {
                AppendInsertOperation(commandStringBuilder, modification, commandPosition++, out var localRequiresTransaction);
                requiresTransaction = requiresTransaction || localRequiresTransaction;
            }

            return readOperations.Count == 0
                ? ResultSetMapping.NoResultSet
                : ResultSetMapping.LastInResultSet;
        }

        if (readOperations.Count == 0)
        {
            // We have no values to read, just use a plain old multi-row INSERT.
            return AppendInsertMultipleRows(
                commandStringBuilder, modificationCommands, writeOperations, out requiresTransaction);
        }

        if (firstCommand.Entries.SelectMany(e => e.EntityType.GetAllBaseTypesInclusive())
            .Any(e => e.IsMemoryOptimized()))
        {
            requiresTransaction = modificationCommands.Count > 1;

            if (!writableOperations.Any(o => o.IsRead && o.IsKey))
            {
                foreach (var modification in modificationCommands)
                {
                    AppendInsertOperation(commandStringBuilder, modification, commandPosition++, out var localRequiresTransaction);
                    requiresTransaction = requiresTransaction || localRequiresTransaction;
                }
            }
            else
            {
                foreach (var modification in modificationCommands)
                {
                    AppendInsertSingleRowWithOutputInto(
                        commandStringBuilder, modification, keyOperations, readOperations, commandPosition++,
                        out var localRequiresTransaction);
                    requiresTransaction = requiresTransaction || localRequiresTransaction;
                }
            }

            return ResultSetMapping.LastInResultSet;
        }

        // We default to using MERGE ... OUTPUT (without INTO), projecting back a synthetic _Position column to know the order back
        // at the client and propagate database-generated values correctly. However, if any triggers are defined, OUTPUT without INTO
        // doesn't work.
        if (!HasAnyTriggers(firstCommand))
        {
            // MERGE ... OUTPUT returns rows whose ordering isn't guaranteed. So this technique projects back a position int with each row,
            // to allow mapping the rows back for value propagation.
            resultsContainPositionMapping = true;

            return AppendMergeWithOutput(
                commandStringBuilder, modificationCommands, writeOperations, readOperations, out requiresTransaction);
        }

        // We have a trigger, so can't use a simple OUTPUT clause.
        // If we have an IDENTITY column, then multiple batched SELECT+INSERTs are faster up to a certain threshold (4), and then
        // MERGE ... OUTPUT INTO is faster.
        if (modificationCommands.Count < MergeIntoMinimumThreshold
            && firstCommand.ColumnModifications.All(
                o =>
                    !o.IsKey
                    || !o.IsRead
                    || o.Property?.GetValueGenerationStrategy(table) == SqlServerValueGenerationStrategy.IdentityColumn))
        {
            requiresTransaction = true;

            foreach (var command in modificationCommands)
            {
                AppendInsertAndSelectOperations(commandStringBuilder, command, commandPosition++, out _);
            }

            return ResultSetMapping.LastInResultSet;
        }

        return AppendMergeWithOutputInto(
            commandStringBuilder, modificationCommands, commandPosition, writeOperations, keyOperations, readOperations,
            out requiresTransaction);
    }

    private ResultSetMapping AppendInsertMultipleRows(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        List<IColumnModification> writeOperations,
        out bool requiresTransaction)
    {
        Check.DebugAssert(writeOperations.Count > 0, $"writeOperations.Count is {writeOperations.Count}");

        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;

        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);
        for (var i = 1; i < modificationCommands.Count; i++)
        {
            commandStringBuilder.AppendLine(",");
            AppendValues(commandStringBuilder, name, schema, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
        }

        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = false;

        return ResultSetMapping.NoResultSet;
    }

    private const string InsertedTableBaseName = "@inserted";
    private const string ToInsertTableAlias = "i";
    private const string PositionColumnName = "_Position";
    private const string PositionColumnDeclaration = "[" + PositionColumnName + "] [int]";
    private const string FullPositionColumnName = ToInsertTableAlias + "." + PositionColumnName;

    private ResultSetMapping AppendMergeWithOutput(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        List<IColumnModification> writeOperations,
        List<IColumnModification> readOperations,
        out bool requiresTransaction)
    {
        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;

        AppendMergeCommandHeader(
            commandStringBuilder,
            name,
            schema,
            ToInsertTableAlias,
            modificationCommands,
            writeOperations,
            PositionColumnName);
        AppendOutputClause(
            commandStringBuilder,
            readOperations,
            FullPositionColumnName);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = false;

        return ResultSetMapping.NotLastInResultSet;
    }

    private ResultSetMapping AppendMergeWithOutputInto(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition,
        List<IColumnModification> writeOperations,
        List<IColumnModification> keyOperations,
        List<IColumnModification> readOperations,
        out bool requiresTransaction)
    {
        AppendDeclareTable(
            commandStringBuilder,
            InsertedTableBaseName,
            commandPosition,
            keyOperations,
            PositionColumnDeclaration);

        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;

        AppendMergeCommandHeader(
            commandStringBuilder,
            name,
            schema,
            ToInsertTableAlias,
            modificationCommands,
            writeOperations,
            PositionColumnName);
        AppendOutputIntoClause(
            commandStringBuilder,
            keyOperations,
            InsertedTableBaseName,
            commandPosition,
            FullPositionColumnName);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        AppendSelectCommand(
            commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema,
            orderColumn: PositionColumnName);

        requiresTransaction = true;

        return ResultSetMapping.NotLastInResultSet;
    }

    private ResultSetMapping AppendInsertMultipleDefaultRows(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        List<IColumnModification> writeableOperations,
        out bool requiresTransaction)
    {
        Check.DebugAssert(writeableOperations.Count > 0, $"writeableOperations.Count is {writeableOperations.Count}");

        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;

        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeableOperations);
        AppendValuesHeader(commandStringBuilder, writeableOperations);
        AppendValues(commandStringBuilder, name, schema, writeableOperations);
        for (var i = 1; i < modificationCommands.Count; i++)
        {
            commandStringBuilder.AppendLine(",");
            AppendValues(commandStringBuilder, name, schema, writeableOperations);
        }

        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = false;

        return ResultSetMapping.NoResultSet;
    }

    private ResultSetMapping AppendInsertMultipleDefaultRowsWithOutputInto(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition,
        List<IColumnModification> writableOperations,
        List<IColumnModification> keyOperations,
        List<IColumnModification> readOperations,
        out bool requiresTransaction)
    {
        AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, keyOperations);

        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;
        AppendInsertCommandHeader(commandStringBuilder, name, schema, writableOperations);
        AppendOutputIntoClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
        AppendValuesHeader(commandStringBuilder, writableOperations);
        AppendValues(commandStringBuilder, name, schema, writableOperations);
        for (var i = 1; i < modificationCommands.Count; i++)
        {
            commandStringBuilder.AppendLine(",");
            AppendValues(commandStringBuilder, name, schema, writableOperations);
        }

        commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

        AppendSelectCommand(commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema);

        requiresTransaction = true;

        return ResultSetMapping.NotLastInResultSet;
    }

    private void AppendMergeCommandHeader(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        string toInsertTableAlias,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        IReadOnlyList<IColumnModification> writeOperations,
        string? additionalColumns = null)
    {
        commandStringBuilder.Append("MERGE ");
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);

        commandStringBuilder
            .Append(" USING (");

        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, writeOperations, "0");
        for (var i = 1; i < modificationCommands.Count; i++)
        {
            commandStringBuilder.AppendLine(",");
            AppendValues(
                commandStringBuilder,
                modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList(),
                i.ToString(CultureInfo.InvariantCulture));
        }

        commandStringBuilder
            .Append(") AS ").Append(toInsertTableAlias)
            .Append(" (")
            .AppendJoin(
                writeOperations,
                SqlGenerationHelper,
                (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName));
        if (additionalColumns != null)
        {
            commandStringBuilder
                .Append(", ")
                .Append(additionalColumns);
        }

        commandStringBuilder
            .Append(')')
            .AppendLine(" ON 1=0")
            .AppendLine("WHEN NOT MATCHED THEN")
            .Append("INSERT ")
            .Append('(')
            .AppendJoin(
                writeOperations,
                SqlGenerationHelper,
                (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName))
            .Append(')');

        AppendValuesHeader(commandStringBuilder, writeOperations);
        commandStringBuilder
            .Append('(')
            .AppendJoin(
                writeOperations,
                (toInsertTableAlias, SqlGenerationHelper),
                static (sb, o, state) =>
                {
                    var (alias, helper) = state;
                    sb.Append(alias).Append('.');
                    helper.DelimitIdentifier(sb, o.ColumnName);
                })
            .Append(')');
    }

    private void AppendValues(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string additionalLiteral)
    {
        if (operations.Count > 0)
        {
            commandStringBuilder
                .Append('(')
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                    {
                        if (o.IsWrite)
                        {
                            helper.GenerateParameterName(sb, o.ParameterName!);
                        }
                        else
                        {
                            sb.Append("DEFAULT");
                        }
                    })
                .Append(", ")
                .Append(additionalLiteral)
                .Append(')');
        }
    }

    private void AppendDeclareTable(
        StringBuilder commandStringBuilder,
        string name,
        int index,
        IReadOnlyList<IColumnModification> operations,
        string? additionalColumns = null)
    {
        commandStringBuilder
            .Append("DECLARE ")
            .Append(name)
            .Append(index)
            .Append(" TABLE (")
            .AppendJoin(
                operations,
                this,
                (sb, o, generator) =>
                {
                    generator.SqlGenerationHelper.DelimitIdentifier(sb, o.ColumnName);
                    sb.Append(' ').Append(GetTypeNameForCopy(o.Property!));
                });

        if (additionalColumns != null)
        {
            commandStringBuilder
                .Append(", ")
                .Append(additionalColumns);
        }

        commandStringBuilder
            .Append(')')
            .AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    private static string GetTypeNameForCopy(IProperty property)
    {
        var typeName = property.GetColumnType();

        return property.ClrType == typeof(byte[])
            && (typeName.Equals("rowversion", StringComparison.OrdinalIgnoreCase)
                || typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
                ? property.IsNullable ? "varbinary(8)" : "binary(8)"
                : typeName;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AppendReturningClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string? additionalValues = null)
        => AppendOutputClause(commandStringBuilder, operations, additionalValues);

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    private void AppendOutputClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string? additionalReadValues = null)
    {
        if (operations.Count > 0 || additionalReadValues is not null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                    {
                        sb.Append("INSERTED.");
                        helper.DelimitIdentifier(sb, o.ColumnName);
                    });

            if (additionalReadValues is not null)
            {
                if (operations.Count > 0)
                {
                    commandStringBuilder.Append(", ");
                }

                commandStringBuilder.Append(additionalReadValues);
            }
        }
    }

    private void AppendOutputIntoClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string tableName,
        int tableIndex,
        string? additionalColumns = null)
    {
        if (operations.Count > 0 || additionalColumns is not null)
        {
            AppendOutputClause(commandStringBuilder, operations, additionalColumns);

            commandStringBuilder.AppendLine()
                .Append("INTO ").Append(tableName).Append(tableIndex);
        }
    }

    private ResultSetMapping AppendInsertSingleRowWithOutputInto(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        IReadOnlyList<IColumnModification> keyOperations,
        IReadOnlyList<IColumnModification> readOperations,
        int commandPosition,
        out bool requiresTransaction)
    {
        var name = command.TableName;
        var schema = command.Schema;
        var operations = command.ColumnModifications;

        var writeOperations = operations.Where(o => o.IsWrite).ToList();

        AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, keyOperations);

        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendOutputIntoClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);
        commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = true;

        return AppendSelectCommand(
            commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema);
    }

    private ResultSetMapping AppendSelectCommand(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> readOperations,
        IReadOnlyList<IColumnModification> keyOperations,
        string insertedTableName,
        int insertedTableIndex,
        string tableName,
        string? schema,
        string? orderColumn = null)
    {
        if (readOperations.SequenceEqual(keyOperations))
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName, "i"))
                .Append(" FROM ")
                .Append(insertedTableName).Append(insertedTableIndex).Append(" i");
        }
        else
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName, "t"))
                .Append(" FROM ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, tableName, schema);
            commandStringBuilder
                .AppendLine(" t")
                .Append("INNER JOIN ")
                .Append(insertedTableName).Append(insertedTableIndex)
                .Append(" i")
                .Append(" ON ")
                .AppendJoin(
                    keyOperations, (sb, c) =>
                    {
                        sb.Append('(');
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "t");
                        sb.Append(" = ");
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "i");
                        sb.Append(')');
                    }, " AND ");
        }

        if (orderColumn != null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("ORDER BY ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, orderColumn, "i");
        }

        commandStringBuilder
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .AppendLine();

        return ResultSetMapping.LastInResultSet;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ResultSetMapping AppendSelectAffectedCountCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        int commandPosition)
    {
        commandStringBuilder
            .Append("SELECT @@ROWCOUNT")
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .AppendLine();

        return ResultSetMapping.LastInResultSet;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        => commandStringBuilder
            .Append("SET NOCOUNT ON")
            .AppendLine(SqlGenerationHelper.StatementTerminator);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void PrependEnsureAutocommit(StringBuilder commandStringBuilder)
    {
        // SQL Server allows turning off autocommit via the IMPLICIT_TRANSACTIONS setting (see
        // https://docs.microsoft.com/sql/t-sql/statements/set-implicit-transactions-transact-sql).
        commandStringBuilder.Insert(0, $"SET IMPLICIT_TRANSACTIONS OFF{SqlGenerationHelper.StatementTerminator}{Environment.NewLine}");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, IColumnModification columnModification)
    {
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
        commandStringBuilder.Append(" = ");

        commandStringBuilder.Append("scope_identity()");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        => commandStringBuilder
            .Append("@@ROWCOUNT = ")
            .Append(expectedRowsAffected.ToString(CultureInfo.InvariantCulture));

    private static bool HasAnyTriggers(IReadOnlyModificationCommand command)
        // Data seeding doesn't provide any entries, so we we don't know if the table has triggers; assume it does to generate SQL
        // that works everywhere.
        => command.Entries.Count == 0
            || command.Table!.Triggers.Any();
}
