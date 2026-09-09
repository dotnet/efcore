// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Used to scaffold new migrations.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IMigrationsScaffolder
{
    /// <summary>
    ///     Scaffolds a new migration.
    /// </summary>
    /// <param name="migrationName">The migration's name.</param>
    /// <param name="rootNamespace">The project's root namespace.</param>
    /// <param name="subNamespace">The migration's sub-namespace.</param>
    /// <param name="language">The project's language.</param>
    /// <param name="dryRun">If <see langword="true" />, then nothing is actually written to disk.</param>
    /// <returns>The scaffolded migration.</returns>
    ScaffoldedMigration ScaffoldMigration(
        string migrationName,
        string? rootNamespace,
        string? subNamespace = null,
        string? language = null,
        bool dryRun = false);

    /// <summary>
    ///     Removes the previous migration.
    /// </summary>
    /// <param name="projectDir">The project's root directory.</param>
    /// <param name="rootNamespace">The project's root namespace.</param>
    /// <param name="force">Don't check to see if the migration has been applied to the database.</param>
    /// <param name="language">The project's language.</param>
    /// <param name="dryRun">If <see langword="true" />, then nothing is actually written to disk.</param>
    /// <returns>The removed migration files.</returns>
    MigrationFiles RemoveMigration(
        string projectDir,
        string? rootNamespace,
        bool force,
        string? language,
        bool dryRun = false);

    /// <summary>
    ///     Saves a scaffolded migration to files.
    /// </summary>
    /// <param name="projectDir">The project's root directory.</param>
    /// <param name="migration">The scaffolded migration.</param>
    /// <param name="outputDir">The directory to put files in. Paths are relative to the project directory.</param>
    /// <param name="dryRun">If <see langword="true" />, then nothing is actually written to disk.</param>
    /// <returns>The saved migrations files.</returns>
    MigrationFiles Save(
        string projectDir,
        ScaffoldedMigration migration,
        string? outputDir,
        bool dryRun = false);
}
