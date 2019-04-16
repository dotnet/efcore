// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to scaffold new migrations.
    /// </summary>
    public interface IMigrationsScaffolder
    {
        /// <summary>
        ///     Scaffolds a new migration.
        /// </summary>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="rootNamespace"> The project's root namespace. </param>
        /// <param name="subNamespace"> The migration's sub-namespace. </param>
        /// <param name="language"> The project's language. </param>
        /// <returns> The scaffolded migration. </returns>
        ScaffoldedMigration ScaffoldMigration(
            [NotNull] string migrationName,
            [NotNull] string rootNamespace,
            [CanBeNull] string subNamespace = null,
            [CanBeNull] string language = null);

        /// <summary>
        ///     Removes the previous migration.
        /// </summary>
        /// <param name="projectDir"> The project's root directory. </param>
        /// <param name="rootNamespace"> The project's root namespace. </param>
        /// <param name="force"> Don't check to see if the migration has been applied to the database. </param>
        /// <param name="language"> The project's language. </param>
        /// <returns> The removed migration files. </returns>
        MigrationFiles RemoveMigration(
            [NotNull] string projectDir,
            [NotNull] string rootNamespace,
            bool force,
            [CanBeNull] string language);

        /// <summary>
        ///     Saves a scaffolded migration to files.
        /// </summary>
        /// <param name="projectDir"> The project's root directory. </param>
        /// <param name="migration"> The scaffolded migration. </param>
        /// <param name="outputDir"> The directory to put files in. Paths are relative to the project directory. </param>
        /// <returns> The saved migrations files. </returns>
        MigrationFiles Save(
            [NotNull] string projectDir,
            [NotNull] ScaffoldedMigration migration,
            [CanBeNull] string outputDir);
    }
}
