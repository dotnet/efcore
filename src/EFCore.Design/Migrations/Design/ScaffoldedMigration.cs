// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Represents a scaffolded migration.
/// </summary>
public class ScaffoldedMigration
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScaffoldedMigration" /> class.
    /// </summary>
    /// <param name="fileExtension">The file extension code files should use.</param>
    /// <param name="previousMigrationId">The previous migration's ID.</param>
    /// <param name="migrationCode">The contents of the migration file.</param>
    /// <param name="migrationId">The migration's ID.</param>
    /// <param name="metadataCode">The contents of the migration metadata file.</param>
    /// <param name="migrationSubNamespace">The migration's sub-namespace.</param>
    /// <param name="snapshotCode">The contents of the model snapshot file.</param>
    /// <param name="snapshotName">The model snapshot's name.</param>
    /// <param name="snapshotSubNamespace">The model snapshot's sub-namespace.</param>
    public ScaffoldedMigration(
        string fileExtension,
        string? previousMigrationId,
        string migrationCode,
        string migrationId,
        string metadataCode,
        string migrationSubNamespace,
        string snapshotCode,
        string snapshotName,
        string snapshotSubNamespace)
    {
        FileExtension = fileExtension;
        PreviousMigrationId = previousMigrationId;
        MigrationCode = migrationCode;
        MigrationId = migrationId;
        MetadataCode = metadataCode;
        MigrationSubNamespace = migrationSubNamespace;
        SnapshotCode = snapshotCode;
        SnapshotName = snapshotName;
        SnapshotSubnamespace = snapshotSubNamespace;
    }

    /// <summary>
    ///     Gets the file extension code files should use.
    /// </summary>
    /// <value> The file extension code files should use. </value>
    public virtual string FileExtension { get; }

    /// <summary>
    ///     Gets the previous migration's ID.
    /// </summary>
    /// <value> The previous migration's ID. </value>
    public virtual string? PreviousMigrationId { get; }

    /// <summary>
    ///     Gets the contents of the migration file.
    /// </summary>
    /// <value> The contents of the migration file. </value>
    public virtual string MigrationCode { get; }

    /// <summary>
    ///     Gets the migration's ID.
    /// </summary>
    /// <value> The migration's ID. </value>
    public virtual string MigrationId { get; }

    /// <summary>
    ///     Gets the contents of the migration metadata file.
    /// </summary>
    /// <value> The contents of the migration metadata file. </value>
    public virtual string MetadataCode { get; }

    /// <summary>
    ///     Gets the migration's sub-namespace.
    /// </summary>
    /// <value> The migration's sub-namespace. </value>
    public virtual string MigrationSubNamespace { get; }

    /// <summary>
    ///     Gets the contents of the model snapshot file.
    /// </summary>
    /// <value> The contents of the model snapshot file. </value>
    public virtual string SnapshotCode { get; }

    /// <summary>
    ///     Gets the model snapshot's name.
    /// </summary>
    /// <value> The model snapshot's name. </value>
    public virtual string SnapshotName { get; }

    /// <summary>
    ///     Gets the model snapshot's sub-namespace.
    /// </summary>
    /// <value> The model snapshot's sub-namespace. </value>
    public virtual string SnapshotSubnamespace { get; }
}
