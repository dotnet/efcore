// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Represents the result of creating and applying a migration at runtime.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public class RuntimeMigrationResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeMigrationResult" /> class.
    /// </summary>
    /// <param name="migrationId">The migration identifier.</param>
    /// <param name="applied">Whether the migration was applied to the database.</param>
    /// <param name="sqlCommands">The SQL commands that were or would be executed.</param>
    /// <param name="migrationFilePath">The path to the migration file, if persisted.</param>
    /// <param name="metadataFilePath">The path to the metadata file, if persisted.</param>
    /// <param name="snapshotFilePath">The path to the snapshot file, if persisted.</param>
    public RuntimeMigrationResult(
        string migrationId,
        bool applied,
        IReadOnlyList<string> sqlCommands,
        string? migrationFilePath = null,
        string? metadataFilePath = null,
        string? snapshotFilePath = null)
    {
        MigrationId = migrationId;
        Applied = applied;
        SqlCommands = sqlCommands;
        MigrationFilePath = migrationFilePath;
        MetadataFilePath = metadataFilePath;
        SnapshotFilePath = snapshotFilePath;
    }

    /// <summary>
    ///     Gets the migration identifier (e.g., "20241223120000_InitialCreate").
    /// </summary>
    public virtual string MigrationId { get; }

    /// <summary>
    ///     Gets a value indicating whether the migration was applied to the database.
    /// </summary>
    /// <remarks>
    ///     This will be <see langword="false" /> if <see cref="RuntimeMigrationOptions.DryRun" />
    ///     was set to <see langword="true" />.
    /// </remarks>
    public virtual bool Applied { get; }

    /// <summary>
    ///     Gets the SQL commands that were executed (or would be executed in dry-run mode).
    /// </summary>
    public virtual IReadOnlyList<string> SqlCommands { get; }

    /// <summary>
    ///     Gets the path to the migration file, if persisted to disk.
    /// </summary>
    public virtual string? MigrationFilePath { get; }

    /// <summary>
    ///     Gets the path to the metadata file, if persisted to disk.
    /// </summary>
    public virtual string? MetadataFilePath { get; }

    /// <summary>
    ///     Gets the path to the model snapshot file, if persisted to disk.
    /// </summary>
    public virtual string? SnapshotFilePath { get; }

    /// <summary>
    ///     Gets a value indicating whether migration files were persisted to disk.
    /// </summary>
    public virtual bool PersistedToDisk
        => MigrationFilePath != null;
}
