// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

/// <summary>
///     A simple model for a database index used when reverse engineering an existing database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class DatabaseIndex : Annotatable
{
    /// <summary>
    ///     The table that contains the index.
    /// </summary>
    public virtual DatabaseTable? Table { get; set; }

    /// <summary>
    ///     The index name.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    ///     The ordered list of columns that make up the index.
    /// </summary>
    public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

    /// <summary>
    ///     Indicates whether or not the index enforces uniqueness.
    /// </summary>
    public virtual bool IsUnique { get; set; }

    /// <summary>
    ///     A set of values indicating whether each corresponding index column has descending sort order.
    /// </summary>
    public virtual IList<bool> IsDescending { get; set; } = new List<bool>();

    /// <summary>
    ///     The filter expression, or <see langword="null" /> if the index has no filter.
    /// </summary>
    public virtual string? Filter { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => Name ?? "<UNKNOWN>";
}
