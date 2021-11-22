// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for creating a new index.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("CREATE INDEX {Name} ON {Table}")]
public class CreateIndexOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     Indicates whether or not the index should enforce uniqueness.
    /// </summary>
    public virtual bool IsUnique { get; set; }

    /// <summary>
    ///     The name of the index.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The schema that contains the index, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The table that contains the index.
    /// </summary>
    public virtual string Table { get; set; } = null!;

    /// <summary>
    ///     The ordered list of column names for the column that make up the index.
    /// </summary>
    public virtual string[] Columns { get; set; } = null!;

    /// <summary>
    ///     An expression to use as the index filter.
    /// </summary>
    public virtual string? Filter { get; set; }

    /// <summary>
    ///     Creates a new <see cref="CreateIndexOperation" /> from the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The operation.</returns>
    public static CreateIndexOperation CreateFrom(ITableIndex index)
    {
        Check.NotNull(index, nameof(index));

        var operation = new CreateIndexOperation
        {
            IsUnique = index.IsUnique,
            Name = index.Name,
            Schema = index.Table.Schema,
            Table = index.Table.Name,
            Columns = index.Columns.Select(p => p.Name).ToArray(),
            Filter = index.Filter
        };
        operation.AddAnnotations(index.GetAnnotations());

        return operation;
    }
}
