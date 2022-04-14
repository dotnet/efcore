// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> to add a new foreign key.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("ALTER TABLE {Table} ADD CONSTRAINT {Name} PRIMARY KEY")]
public class AddPrimaryKeyOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The table to which the key should be added.
    /// </summary>
    public virtual string Table { get; set; } = null!;

    /// <summary>
    ///     The name of the foreign key constraint.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The ordered-list of column names for the columns that make up the primary key.
    /// </summary>
    public virtual string[] Columns { get; set; } = null!;

    /// <summary>
    ///     Creates a new <see cref="AddPrimaryKeyOperation" /> from the specified primary key.
    /// </summary>
    /// <param name="primaryKey">The primary key.</param>
    /// <returns>The operation.</returns>
    public static AddPrimaryKeyOperation CreateFrom(IPrimaryKeyConstraint primaryKey)
    {
        Check.NotNull(primaryKey, nameof(primaryKey));

        var operation = new AddPrimaryKeyOperation
        {
            Schema = primaryKey.Table.Schema,
            Table = primaryKey.Table.Name,
            Name = primaryKey.Name,
            Columns = primaryKey.Columns.Select(c => c.Name).ToArray()
        };

        operation.AddAnnotations(primaryKey.GetAnnotations());

        return operation;
    }
}
