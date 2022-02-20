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
public class SqlServerUpdateSqlGenerator : UpdateSqlGenerator, ISqlServerUpdateSqlGenerator
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ResultSetMapping AppendBulkInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition,
        out bool requiresTransaction)
    {
        var table = StoreObjectIdentifier.Table(modificationCommands[0].TableName, modificationCommands[0].Schema);
        if (modificationCommands.Count == 1)
        {
            return modificationCommands[0].ColumnModifications.All(
                o =>
                    !o.IsKey
                    || !o.IsRead
                    || o.Property?.GetValueGenerationStrategy(table) == SqlServerValueGenerationStrategy.IdentityColumn)
                // Do a regular INSERT+SELECT for IDENTITY, but not if there are any non-IDENTITY generated columns
                ? AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition, out requiresTransaction)
                // If we have a non-identity generated column, do INSERT ... OUTPUT INTO @inserted; SELECT ... FROM @inserted
                : AppendInsertOperationWithServerKeys(
                    commandStringBuilder,
                    modificationCommands[0],
                    modificationCommands[0].ColumnModifications.Where(o => o.IsKey).ToList(),
                    modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList(),
                    commandPosition,
                    out requiresTransaction);
        }

        var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
        var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();
        var keyOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsKey).ToList();

        var defaultValuesOnly = writeOperations.Count == 0;
        var writableOperations = modificationCommands[0].ColumnModifications
            .Where(o =>
                o.Property?.GetValueGenerationStrategy(table) != SqlServerValueGenerationStrategy.IdentityColumn
                && o.Property?.GetComputedColumnSql() is null
                && o.Property?.GetColumnType() is not "rowversion" and not "timestamp")
            .ToList();

        if (defaultValuesOnly)
        {
            if (writableOperations.Count == 0
                || readOperations.Count == 0)
            {
                requiresTransaction = false;
                foreach (var modification in modificationCommands)
                {
                    AppendInsertOperation(commandStringBuilder, modification, commandPosition, out var localRequiresTransaction);
                    requiresTransaction = requiresTransaction || localRequiresTransaction;
                }

                return readOperations.Count == 0
                    ? ResultSetMapping.NoResultSet
                    : ResultSetMapping.LastInResultSet;
            }

            if (writableOperations.Count > 1)
            {
                writableOperations.RemoveRange(1, writableOperations.Count - 1);
            }
        }

        if (readOperations.Count == 0)
        {
            return AppendBulkInsertWithoutServerValues(
                commandStringBuilder, modificationCommands, writeOperations, out requiresTransaction);
        }

        if (defaultValuesOnly)
        {
            return AppendBulkInsertWithServerValuesOnly(
                commandStringBuilder, modificationCommands, commandPosition, writableOperations, keyOperations, readOperations,
                out requiresTransaction);
        }

        if (modificationCommands[0].Entries.SelectMany(e => e.EntityType.GetAllBaseTypesInclusive())
            .Any(e => e.IsMemoryOptimized()))
        {
            requiresTransaction = false;

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
                    AppendInsertOperationWithServerKeys(
                        commandStringBuilder, modification, keyOperations, readOperations, commandPosition++,
                        out var localRequiresTransaction);
                    requiresTransaction = requiresTransaction || localRequiresTransaction;
                }
            }

            return ResultSetMapping.LastInResultSet;
        }

        return AppendBulkInsertWithServerValues(
            commandStringBuilder, modificationCommands, commandPosition, writeOperations, keyOperations, readOperations,
            out requiresTransaction);
    }

    private ResultSetMapping AppendBulkInsertWithoutServerValues(
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
            AppendValues(
                commandStringBuilder, name, schema, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
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

    private ResultSetMapping AppendBulkInsertWithServerValues(
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
        AppendOutputClause(
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

    private ResultSetMapping AppendBulkInsertWithServerValuesOnly(
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
        AppendOutputClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
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
            .AppendLine("WHEN NOT MATCHED THEN");

        commandStringBuilder
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

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    private void AppendOutputClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string tableName,
        int tableIndex,
        string? additionalColumns = null)
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

        if (additionalColumns != null)
        {
            commandStringBuilder
                .Append(", ").Append(additionalColumns);
        }

        commandStringBuilder.AppendLine()
            .Append("INTO ").Append(tableName).Append(tableIndex);
    }

    private ResultSetMapping AppendInsertOperationWithServerKeys(
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
        AppendOutputClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
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
}
