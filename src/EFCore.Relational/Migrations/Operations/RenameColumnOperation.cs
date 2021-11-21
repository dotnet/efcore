// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for renaming an existing column.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("ALTER TABLE {Table} RENAME COLUMN {Name} TO {NewName}")]
public class RenameColumnOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     The old name of the column.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The name of the table that contains the column.
    /// </summary>
    public virtual string Table { get; set; } = null!;

    /// <summary>
    ///     The new name for the column.
    /// </summary>
    public virtual string NewName { get; set; } = null!;
}
