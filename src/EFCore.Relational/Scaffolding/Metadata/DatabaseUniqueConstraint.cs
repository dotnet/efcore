// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

/// <summary>
///     A simple model for a database unique constraint used when reverse engineering an existing database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class DatabaseUniqueConstraint : Annotatable
{
    /// <summary>
    ///     The table on which the unique constraint is defined.
    /// </summary>
    public virtual DatabaseTable Table { get; set; } = null!;

    /// <summary>
    ///     The name of the constraint.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    ///     The ordered list of columns that make up the constraint.
    /// </summary>
    public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

    /// <inheritdoc />
    public override string ToString()
        => Name ?? "<UNKNOWN>";
}
