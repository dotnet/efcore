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
    private string[]? _columns;
    private bool[]? _isDescending;

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
    public virtual string[] Columns
    {
        get => _columns!;
        set
        {
            if (_isDescending is not null && _isDescending.Length > 0 && value.Length != _isDescending.Length)
            {
                throw new ArgumentException(RelationalStrings.CreateIndexOperationWithInvalidSortOrder(_isDescending.Length, value.Length));
            }

            _columns = value;
        }
    }

    /// <summary>
    ///     Indicates whether or not the index should enforce uniqueness.
    /// </summary>
    public virtual bool IsUnique { get; set; }

    /// <summary>
    ///     A set of values indicating whether each corresponding index column has descending sort order.
    /// </summary>
    public virtual bool[]? IsDescending
    {
        get => _isDescending;
        set
        {
            if (value is not null && value.Length > 0 && _columns is not null && value.Length != _columns.Length)
            {
                throw new ArgumentException(RelationalStrings.CreateIndexOperationWithInvalidSortOrder(value.Length, _columns.Length));
            }

            _isDescending = value;
        }
    }

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
            Name = index.Name,
            Schema = index.Table.Schema,
            Table = index.Table.Name,
            Columns = index.Columns.Select(p => p.Name).ToArray(),
            IsUnique = index.IsUnique,
            IsDescending = index.IsDescending?.ToArray(),
            Filter = index.Filter
        };
        operation.AddAnnotations(index.GetAnnotations());

        return operation;
    }
}
