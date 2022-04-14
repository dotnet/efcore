// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

/// <summary>
///     A simple model for a database used when reverse engineering an existing database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class DatabaseModel : Annotatable
{
    /// <summary>
    ///     The database name, or <see langword="null" /> if none is set.
    /// </summary>
    public virtual string? DatabaseName { get; set; }

    /// <summary>
    ///     The database schema, or <see langword="null" /> to use the default schema.
    /// </summary>
    public virtual string? DefaultSchema { get; set; }

    /// <summary>
    ///     The database collation, or <see langword="null" /> if none is set.
    /// </summary>
    public virtual string? Collation { get; set; }

    /// <summary>
    ///     The list of tables in the database.
    /// </summary>
    public virtual IList<DatabaseTable> Tables { get; } = new List<DatabaseTable>();

    /// <summary>
    ///     The list of sequences in the database.
    /// </summary>
    public virtual IList<DatabaseSequence> Sequences { get; } = new List<DatabaseSequence>();
}
