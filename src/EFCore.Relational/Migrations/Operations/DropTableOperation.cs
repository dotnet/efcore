// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for dropping an existing table.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("DROP TABLE {Name}")]
public class DropTableOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     Creates a new <see cref="DropTableOperation" />.
    /// </summary>
    // ReSharper disable once VirtualMemberCallInConstructor
    public DropTableOperation()
    {
        IsDestructiveChange = true;
    }

    /// <summary>
    ///     The name of the table.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <inheritdoc />
    string ITableMigrationOperation.Table
        => Name;
}
