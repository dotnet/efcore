// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

/// <summary>
///     A simple model for a database primary key used when reverse engineering an existing database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class DatabasePrimaryKey : Annotatable
{
    /// <summary>
    ///     The table on which the primary key is defined.
    /// </summary>
    public virtual DatabaseTable? Table { get; set; }

    /// <summary>
    ///     The name of the primary key.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    ///     The ordered list of columns that make up the primary key.
    /// </summary>
    public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

    /// <inheritdoc />
    public override string ToString()
        => Name ?? "<UNKNOWN>";
}
