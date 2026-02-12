// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for creating a new table.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("CREATE TABLE {Name}")]
public class CreateTableOperation : TableOperation
{
    /// <summary>
    ///     The <see cref="AddPrimaryKeyOperation" /> representing the creation of the primary key for the table.
    /// </summary>
    public virtual AddPrimaryKeyOperation? PrimaryKey { get; set; }

    /// <summary>
    ///     An ordered list of <see cref="AddColumnOperation" /> for adding columns to the table.
    /// </summary>
    public virtual List<AddColumnOperation> Columns { get; } = [];

    /// <summary>
    ///     A list of <see cref="AddForeignKeyOperation" /> for creating foreign key constraints in the table.
    /// </summary>
    public virtual List<AddForeignKeyOperation> ForeignKeys { get; } = [];

    /// <summary>
    ///     A list of <see cref="AddUniqueConstraintOperation" /> for creating unique constraints in the table.
    /// </summary>
    public virtual List<AddUniqueConstraintOperation> UniqueConstraints { get; } = [];

    /// <summary>
    ///     A list of <see cref="AddCheckConstraintOperation" /> for creating check constraints in the table.
    /// </summary>
    public virtual List<AddCheckConstraintOperation> CheckConstraints { get; } = [];
}
