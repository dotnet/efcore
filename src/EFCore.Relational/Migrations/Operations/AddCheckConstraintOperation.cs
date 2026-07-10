// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for creating a new check constraint.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("ALTER TABLE {Table} ADD CONSTRAINT {Name} CHECK")]
public class AddCheckConstraintOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     The name of the check constraint.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The table of the check constraint.
    /// </summary>
    public virtual string Table { get; set; } = null!;

    /// <summary>
    ///     The table schema that contains the check constraint, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The logical sql expression used in a CHECK constraint and returns TRUE or FALSE.
    ///     SQL used with CHECK constraints cannot reference another table
    ///     but can reference other columns in the same table for the same row.
    ///     The expression cannot reference an alias data type.
    /// </summary>
    public virtual string Sql { get; set; } = null!;

    /// <summary>
    ///     Creates a new <see cref="AddCheckConstraintOperation" /> from the specified check constraint.
    /// </summary>
    /// <param name="checkConstraint">The check constraint.</param>
    /// <returns>The operation.</returns>
    public static AddCheckConstraintOperation CreateFrom(ICheckConstraint checkConstraint)
    {
        Check.NotNull(checkConstraint, nameof(checkConstraint));

        var operation = new AddCheckConstraintOperation
        {
            Name = checkConstraint.Name!,
            Sql = checkConstraint.Sql,
            Schema = checkConstraint.EntityType.GetSchema(),
            Table = checkConstraint.EntityType.GetTableName()!
        };

        return operation;
    }
}
