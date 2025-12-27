// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Options for creating and applying migrations at runtime.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public class RuntimeMigrationOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether to persist migration files to disk.
    ///     Default is <see langword="true" />.
    /// </summary>
    /// <remarks>
    ///     When set to <see langword="true" />, migration source files (.cs) will be written
    ///     to the output directory. When <see langword="false" />, the migration is compiled
    ///     and applied in-memory without persisting source files.
    /// </remarks>
    public virtual bool PersistToDisk { get; set; } = true;

    /// <summary>
    ///     Gets or sets the output directory for migration files.
    /// </summary>
    /// <remarks>
    ///     This is relative to the project directory. If not specified, defaults to "Migrations".
    /// </remarks>
    public virtual string? OutputDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the namespace for the generated migration.
    /// </summary>
    /// <remarks>
    ///     If not specified, the namespace is inferred from the context type and output directory.
    /// </remarks>
    public virtual string? Namespace { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to run in dry-run mode.
    /// </summary>
    /// <remarks>
    ///     When set to <see langword="true" />, the migration is created and compiled but not
    ///     applied to the database. The generated SQL commands are still returned in the result.
    /// </remarks>
    public virtual bool DryRun { get; set; }

    /// <summary>
    ///     Gets or sets the project directory for resolving relative paths.
    /// </summary>
    /// <remarks>
    ///     If not specified, the current working directory is used.
    /// </remarks>
    public virtual string? ProjectDirectory { get; set; }
}
