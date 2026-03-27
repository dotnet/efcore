// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Represents the file generated for a migration.
/// </summary>
public class MigrationFiles
{
    /// <summary>
    ///     Gets or sets the path to the migration file.
    /// </summary>
    /// <value>The path to the migration file.</value>
    public virtual string? MigrationFile { get; set; }

    /// <summary>
    ///     Gets or sets the path to the migration metadata file.
    /// </summary>
    /// <value>The path to the migration metadata file.</value>
    public virtual string? MetadataFile { get; set; }

    /// <summary>
    ///     Gets or sets the path to the model snapshot file.
    /// </summary>
    /// <value>The path to the model snapshot file.</value>
    public virtual string? SnapshotFile { get; set; }

    /// <summary>
    ///     Gets or sets the scaffolded migration.
    /// </summary>
    /// <value>The scaffolded migration.</value>
    public virtual ScaffoldedMigration? Migration { get; set; }
}
