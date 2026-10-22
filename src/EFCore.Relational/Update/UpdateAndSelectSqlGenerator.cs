// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         A base class for the <see cref="IUpdateSqlGenerator" /> service that is typically inherited from by database providers.
///         The implementation uses a separate SELECT query after the update SQL to retrieve any database-generated values or for
///         concurrency checking.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance is used by many
///         <see cref="DbContext" /> instances. The implementation must be thread-safe. This service cannot depend on services registered
///         as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see> for more
///         information and examples.
///     </para>
/// </remarks>
public abstract class UpdateAndSelectSqlGenerator : UpdateSqlGenerator
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected UpdateAndSelectSqlGenerator(UpdateSqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
        => AppendInsertAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

    /// <summary>
    ///     Appends SQL for inserting a row to the commands being built, via an INSERT followed by an optional SELECT to retrieve any
    ///     database-generated values.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="command">The command that represents the delete operation.</param>
    /// <param name="commandPosition">The ordinal of this command in the batch.</param>
    /// <param name="requiresTransaction">Returns whether the SQL appended must be executed in a transaction to work correctly.</param>
    /// <returns>The <see cref="ResultSetMapping" /> for the command.</returns>
    protected virtual ResultSetMapping AppendInsertAndSelectOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        var name = command.TableName;
        var schema = command.Schema;
        var operations = command.ColumnModifications;

        var writeOperations = operations.Where(o => o.IsWrite).ToList();
        var readOperations = operations.Where(o => o.IsRead).ToList();

        AppendInsertCommand(commandStringBuilder, name, schema, writeOperations, readOperations: []);

        if (readOperations.Count > 0)
        {
            var keyOperations = operations.Where(o => o.IsKey).ToList();

            requiresTransaction = true;

            return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
        }

        requiresTransaction = false;

        return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
    }

    /// <inheritdoc />
    public override ResultSetMapping AppendUpdateOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
        => AppendUpdateAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

    /// <summary>
    ///     Appends SQL for updating a row to the commands being built, via an UPDATE followed by a SELECT to retrieve any
    ///     database-generated values or for concurrency checking.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="command">The command that represents the delete operation.</param>
    /// <param name="commandPosition">The ordinal of this command in the batch.</param>
    /// <param name="requiresTransaction">Returns whether the SQL appended must be executed in a transaction to work correctly.</param>
    /// <returns>The <see cref="ResultSetMapping" /> for the command.</returns>
    protected virtual ResultSetMapping AppendUpdateAndSelectOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        var name = command.TableName;
        var schema = command.Schema;
        var operations = command.ColumnModifications;

        var writeOperations = operations.Where(o => o.IsWrite).ToList();
        var conditionOperations = operations.Where(o => o.IsCondition).ToList();
        var readOperations = operations.Where(o => o.IsRead).ToList();

        AppendUpdateCommand(commandStringBuilder, name, schema, writeOperations, [], conditionOperations);

        if (readOperations.Count > 0)
        {
            var keyOperations = operations.Where(o => o.IsKey).ToList();

            requiresTransaction = true;

            return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
        }

        requiresTransaction = false;

        return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
    }

    /// <inheritdoc />
    public override ResultSetMapping AppendDeleteOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
        => AppendDeleteAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

    /// <summary>
    ///     Appends SQL for updating a row to the commands being built, via a DELETE followed by a SELECT for concurrency checking.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="command">The command that represents the delete operation.</param>
    /// <param name="commandPosition">The ordinal of this command in the batch.</param>
    /// <param name="requiresTransaction">Returns whether the SQL appended must be executed in a transaction to work correctly.</param>
    /// <returns>The <see cref="ResultSetMapping" /> for the command.</returns>
    protected virtual ResultSetMapping AppendDeleteAndSelectOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        var name = command.TableName;
        var schema = command.Schema;
        var operations = command.ColumnModifications;

        var conditionOperations = operations.Where(o => o.IsCondition).ToList();

        requiresTransaction = false;

        AppendDeleteCommand(commandStringBuilder, name, schema, [], conditionOperations);

        return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
    }

    /// <summary>
    ///     Appends a SQL command for selecting affected data.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The table schema, or <see langword="null" /> to use the default schema.</param>
    /// <param name="readOperations">The operations representing the data to be read.</param>
    /// <param name="conditionOperations">The operations used to generate the <c>WHERE</c> clause for the select.</param>
    /// <param name="commandPosition">The ordinal of the command for which rows affected it being returned.</param>
    /// <returns>The <see cref="ResultSetMapping" /> for this command.</returns>
    protected virtual ResultSetMapping AppendSelectAffectedCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> readOperations,
        IReadOnlyList<IColumnModification> conditionOperations,
        int commandPosition)
    {
        AppendSelectCommandHeader(commandStringBuilder, readOperations);
        AppendFromClause(commandStringBuilder, name, schema);
        AppendWhereAffectedClause(commandStringBuilder, conditionOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator)
            .AppendLine();

        return ResultSetMapping.LastInResultSet;
    }

    /// <summary>
    ///     Appends a SQL fragment for starting a <c>SELECT</c>.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="operations">The operations representing the data to be read.</param>
    protected virtual void AppendSelectCommandHeader(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations)
        => commandStringBuilder
            .Append("SELECT ")
            .AppendJoin(
                operations,
                SqlGenerationHelper,
                (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName));

    /// <summary>
    ///     Appends a SQL fragment for starting a <c>FROM</c> clause.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The table schema, or <see langword="null" /> to use the default schema.</param>
    protected virtual void AppendFromClause(
        StringBuilder commandStringBuilder,
        string name,
        string? schema)
    {
        commandStringBuilder
            .AppendLine()
            .Append("FROM ");
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
    }

    /// <summary>
    ///     Appends a <c>WHERE</c> clause involving rows affected.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="operations">The operations from which to build the conditions.</param>
    protected virtual void AppendWhereAffectedClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations)
    {
        commandStringBuilder
            .AppendLine()
            .Append("WHERE ");

        AppendRowsAffectedWhereCondition(commandStringBuilder, 1);

        if (operations.Count > 0)
        {
            commandStringBuilder
                .Append(" AND ")
                .AppendJoin(
                    operations, (sb, v) =>
                    {
                        if (v is { IsKey: true, IsRead: false })
                        {
                            AppendWhereCondition(sb, v, v.UseOriginalValueParameter);
                            return true;
                        }

                        if (IsIdentityOperation(v))
                        {
                            AppendIdentityWhereCondition(sb, v);
                            return true;
                        }

                        return false;
                    }, " AND ");
        }
    }

    /// <summary>
    ///     Returns a value indicating whether the given modification represents an auto-incrementing column.
    /// </summary>
    /// <param name="modification">The column modification.</param>
    /// <returns><see langword="true" /> if the given modification represents an auto-incrementing column.</returns>
    protected virtual bool IsIdentityOperation(IColumnModification modification)
        => modification is { IsKey: true, IsRead: true };

    /// <summary>
    ///     Appends a <c>WHERE</c> condition checking rows affected.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="expectedRowsAffected">The expected number of rows affected.</param>
    protected abstract void AppendRowsAffectedWhereCondition(
        StringBuilder commandStringBuilder,
        int expectedRowsAffected);

    /// <summary>
    ///     Appends a <c>WHERE</c> condition for the identity (i.e. key value) of the given column.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="columnModification">The column for which the condition is being generated.</param>
    protected abstract void AppendIdentityWhereCondition(
        StringBuilder commandStringBuilder,
        IColumnModification columnModification);

    /// <summary>
    ///     Appends a SQL command for selecting the number of rows affected.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="name">The name of the table.</param>
    /// <param name="schema">The table schema, or <see langword="null" /> to use the default schema.</param>
    /// <param name="commandPosition">The ordinal of the command for which rows affected it being returned.</param>
    /// <returns>The <see cref="ResultSetMapping" /> for this command.</returns>
    protected abstract ResultSetMapping AppendSelectAffectedCountCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        int commandPosition);
}
