// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

/// <summary>
///     A simple model for a database foreign key constraint used when reverse engineering an existing database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class DatabaseForeignKey : Annotatable
{
    /// <summary>
    ///     The table that contains the foreign key constraint.
    /// </summary>
    public virtual DatabaseTable Table { get; set; } = null!;

    /// <summary>
    ///     The table to which the columns are constrained.
    /// </summary>
    public virtual DatabaseTable PrincipalTable { get; set; } = null!;

    /// <summary>
    ///     The ordered list of columns that are constrained.
    /// </summary>
    public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

    /// <summary>
    ///     The ordered list of columns in the <see cref="PrincipalTable" /> to which the <see cref="Columns" />
    ///     of the foreign key are constrained.
    /// </summary>
    public virtual IList<DatabaseColumn> PrincipalColumns { get; } = new List<DatabaseColumn>();

    /// <summary>
    ///     The foreign key constraint name.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    ///     The action performed by the database when a row constrained by this foreign key
    ///     is deleted, or <see langword="null" /> if there is no action defined.
    /// </summary>
    public virtual ReferentialAction? OnDelete { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => Name ?? "<UNKNOWN>";
}
