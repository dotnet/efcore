// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> to add a new unique constraint.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("ALTER TABLE {Table} ADD CONSTRAINT {Name} UNIQUE")]
public class AddUniqueConstraintOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The table to which the constraint should be added.
    /// </summary>
    public virtual string Table { get; set; } = null!;

    /// <summary>
    ///     The name of the constraint.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The ordered-list of column names for the columns that make up the constraint.
    /// </summary>
    public virtual string[] Columns { get; set; } = null!;

    /// <summary>
    ///     Creates a new <see cref="AddUniqueConstraintOperation" /> from the specified unique constraint.
    /// </summary>
    /// <param name="uniqueConstraint">The unique constraint.</param>
    /// <returns>The operation.</returns>
    public static AddUniqueConstraintOperation CreateFrom(IUniqueConstraint uniqueConstraint)
    {
        Check.NotNull(uniqueConstraint, nameof(uniqueConstraint));

        var operation = new AddUniqueConstraintOperation
        {
            Schema = uniqueConstraint.Table.Schema,
            Table = uniqueConstraint.Table.Name,
            Name = uniqueConstraint.Name,
            Columns = uniqueConstraint.Columns.Select(c => c.Name).ToArray()
        };
        operation.AddAnnotations(uniqueConstraint.GetAnnotations());

        return operation;
    }
}
