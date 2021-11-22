// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for deleting seed data from an existing table.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("DELETE FROM {Table}")]
public class DeleteDataOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     The table from which data will be deleted.
    /// </summary>
    public virtual string Table { get; set; } = null!;

    /// <summary>
    ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     A list of column names that represent the columns that will be used to identify
    ///     the rows that should be deleted.
    /// </summary>
    public virtual string[] KeyColumns { get; set; } = null!;

    /// <summary>
    ///     A list of store types for the columns that will be used to identify
    ///     the rows that should be deleted.
    /// </summary>
    public virtual string[]? KeyColumnTypes { get; set; }

    /// <summary>
    ///     The rows to be deleted, represented as a list of key value arrays where each
    ///     value in the array corresponds to a column in the <see cref="KeyColumns" /> property.
    /// </summary>
    public virtual object?[,] KeyValues { get; set; } = null!;
}
