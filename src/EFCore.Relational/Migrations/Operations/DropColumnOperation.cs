// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for dropping an existing column.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("ALTER TABLE {Table} DROP COLUMN {Name}")]
public class DropColumnOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     Creates a new instance of the <see cref="DropColumnOperation" />.
    /// </summary>
    // ReSharper disable once VirtualMemberCallInConstructor
    public DropColumnOperation()
    {
        IsDestructiveChange = true;
    }

    /// <summary>
    ///     The name of the column.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The table that contains that column.
    /// </summary>
    public virtual string Table { get; set; } = null!;
}
