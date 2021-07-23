// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to generate code for migrations.
    /// </summary>
    public interface IMigrationsCodeGenerator : ILanguageBasedService
    {
        /// <summary>
        ///     Generates the migration metadata code.
        /// </summary>
        /// <param name="migrationNamespace"> The migration's namespace. </param>
        /// <param name="contextType"> The migration's <see cref="DbContext" /> type. </param>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="migrationId"> The migration's ID. </param>
        /// <param name="targetModel"> The migration's target model. </param>
        /// <returns> The migration metadata code. </returns>
        string GenerateMetadata(
            string? migrationNamespace,
            Type contextType,
            string migrationName,
            string migrationId,
            IModel targetModel);

        /// <summary>
        ///     Generates the migration code.
        /// </summary>
        /// <param name="migrationNamespace"> The migration's namespace. </param>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="upOperations"> The migration's up operations. </param>
        /// <param name="downOperations"> The migration's down operations. </param>
        /// <returns> The migration code. </returns>
        string GenerateMigration(
            string? migrationNamespace,
            string migrationName,
            IReadOnlyList<MigrationOperation> upOperations,
            IReadOnlyList<MigrationOperation> downOperations);

        /// <summary>
        ///     Generates the model snapshot code.
        /// </summary>
        /// <param name="modelSnapshotNamespace"> The model snapshot's namespace. </param>
        /// <param name="contextType"> The model snapshot's <see cref="DbContext" /> type. </param>
        /// <param name="modelSnapshotName"> The model snapshot's name. </param>
        /// <param name="model"> The model. </param>
        /// <returns> The model snapshot code. </returns>
        string GenerateSnapshot(
            string? modelSnapshotNamespace,
            Type contextType,
            string modelSnapshotName,
            IModel model);

        /// <summary>
        ///     Gets the file extension code files should use.
        /// </summary>
        /// <value> The file extension. </value>
        string FileExtension { get; }
    }
}
