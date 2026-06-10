// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

/// <summary>
///     A builder for <see cref="CreateTableOperation" /> operations.
/// </summary>
/// <typeparam name="TColumns">Type of a typically anonymous type for building columns.</typeparam>
public class CreateTableBuilder<TColumns> : OperationBuilder<CreateTableOperation>
{
    private readonly IReadOnlyDictionary<PropertyInfo, AddColumnOperation> _columnMap;

    /// <summary>
    ///     Constructs a new builder for the given <see cref="CreateTableOperation" /> and
    ///     with the given map of <see cref="AddColumnOperation" /> operations for columns.
    /// </summary>
    /// <param name="operation">The <see cref="CreateTableOperation" />.</param>
    /// <param name="columnMap">The map of CLR properties to <see cref="AddColumnOperation" />s.</param>
    public CreateTableBuilder(
        CreateTableOperation operation,
        IReadOnlyDictionary<PropertyInfo, AddColumnOperation> columnMap)
        : base(operation)
    {
        Check.NotNull(columnMap, nameof(columnMap));

        _columnMap = columnMap;
    }

    /// <summary>
    ///     Configures a single-column foreign key on the table.
    /// </summary>
    /// <param name="name">The foreign key constraint name.</param>
    /// <param name="column">The column used for the foreign key.</param>
    /// <param name="principalTable">The table to which the foreign key is constrained.</param>
    /// <param name="principalColumn">The column to which the foreign key column is constrained.</param>
    /// <param name="principalSchema">The schema that contains the table to which the foreign key is constrained.</param>
    /// <param name="onUpdate">The <see cref="ReferentialAction" /> to use for updates.</param>
    /// <param name="onDelete">The <see cref="ReferentialAction" /> to use for deletes.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual OperationBuilder<AddForeignKeyOperation> ForeignKey(
        string name,
        Expression<Func<TColumns, object>> column,
        string principalTable,
        string? principalColumn = null,
        string? principalSchema = null,
        ReferentialAction onUpdate = ReferentialAction.NoAction,
        ReferentialAction onDelete = ReferentialAction.NoAction)
        => ForeignKey(
            name,
            column,
            principalTable,
            principalColumn == null ? null : [principalColumn],
            principalSchema,
            onUpdate,
            onDelete);

    /// <summary>
    ///     Configures a multiple-column (composite) foreign key on the table.
    /// </summary>
    /// <param name="name">The foreign key constraint name.</param>
    /// <param name="columns">The columns used for the foreign key.</param>
    /// <param name="principalTable">The table to which the foreign key is constrained.</param>
    /// <param name="principalColumns">The columns to which the foreign key column is constrained.</param>
    /// <param name="principalSchema">The schema that contains the table to which the foreign key is constrained.</param>
    /// <param name="onUpdate">The <see cref="ReferentialAction" /> to use for updates.</param>
    /// <param name="onDelete">The <see cref="ReferentialAction" /> to use for deletes.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual OperationBuilder<AddForeignKeyOperation> ForeignKey(
        string name,
        Expression<Func<TColumns, object>> columns,
        string principalTable,
        string[]? principalColumns,
        string? principalSchema = null,
        ReferentialAction onUpdate = ReferentialAction.NoAction,
        ReferentialAction onDelete = ReferentialAction.NoAction)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(columns, nameof(columns));
        Check.NotEmpty(principalTable, nameof(principalTable));

        var operation = new AddForeignKeyOperation
        {
            Schema = Operation.Schema,
            Table = Operation.Name,
            Name = name,
            Columns = Map(columns),
            PrincipalSchema = principalSchema,
            PrincipalTable = principalTable,
            PrincipalColumns = principalColumns,
            OnUpdate = onUpdate,
            OnDelete = onDelete
        };
        Operation.ForeignKeys.Add(operation);

        return new OperationBuilder<AddForeignKeyOperation>(operation);
    }

    /// <summary>
    ///     Configures a primary key on the table.
    /// </summary>
    /// <param name="name">The primary key constraint name.</param>
    /// <param name="columns">The columns that make up the primary key.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual OperationBuilder<AddPrimaryKeyOperation> PrimaryKey(
        string name,
        Expression<Func<TColumns, object>> columns)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(columns, nameof(columns));

        var operation = new AddPrimaryKeyOperation
        {
            Schema = Operation.Schema,
            Table = Operation.Name,
            Name = name,
            Columns = Map(columns)
        };
        // TODO: Throw if already set?
        Operation.PrimaryKey = operation;

        return new OperationBuilder<AddPrimaryKeyOperation>(operation);
    }

    /// <summary>
    ///     Configures a unique constraint on the table.
    /// </summary>
    /// <param name="name">The constraint name.</param>
    /// <param name="columns">The columns that make up the constraint.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual OperationBuilder<AddUniqueConstraintOperation> UniqueConstraint(
        string name,
        Expression<Func<TColumns, object>> columns)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(columns, nameof(columns));

        var operation = new AddUniqueConstraintOperation
        {
            Schema = Operation.Schema,
            Table = Operation.Name,
            Name = name,
            Columns = Map(columns)
        };
        Operation.UniqueConstraints.Add(operation);

        return new OperationBuilder<AddUniqueConstraintOperation>(operation);
    }

    /// <summary>
    ///     Configures a check constraint on the table.
    /// </summary>
    /// <param name="name">The constraint name.</param>
    /// <param name="sql">The sql expression used in the CHECK constraint.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual OperationBuilder<AddCheckConstraintOperation> CheckConstraint(
        string name,
        string sql)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(sql, nameof(sql));

        var operation = new AddCheckConstraintOperation
        {
            Schema = Operation.Schema,
            Table = Operation.Name,
            Name = name,
            Sql = sql
        };
        Operation.CheckConstraints.Add(operation);

        return new OperationBuilder<AddCheckConstraintOperation>(operation);
    }

    /// <summary>
    ///     Annotates the operation with the given name/value pair.
    /// </summary>
    /// <param name="name">The annotation name.</param>
    /// <param name="value">The annotation value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public new virtual CreateTableBuilder<TColumns> Annotation(string name, object value)
        => (CreateTableBuilder<TColumns>)base.Annotation(name, value);

    private string[] Map(LambdaExpression columns)
        => columns.GetPropertyAccessList().Select(c => _columnMap[c].Name).ToArray();
}
