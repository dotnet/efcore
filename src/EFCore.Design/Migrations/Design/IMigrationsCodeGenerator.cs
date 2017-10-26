// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
            [NotNull] string migrationNamespace,
            [NotNull] Type contextType,
            [NotNull] string migrationName,
            [NotNull] string migrationId,
            [NotNull] IModel targetModel);

        /// <summary>
        ///     Generates the migration code.
        /// </summary>
        /// <param name="migrationNamespace"> The migration's namespace. </param>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="upOperations"> The migration's up operations. </param>
        /// <param name="downOperations"> The migration's down operations. </param>
        /// <returns> The migration code. </returns>
        string GenerateMigration(
            [NotNull] string migrationNamespace,
            [NotNull] string migrationName,
            [NotNull] IReadOnlyList<MigrationOperation> upOperations,
            [NotNull] IReadOnlyList<MigrationOperation> downOperations);

        /// <summary>
        ///     Generates the model snapshot code.
        /// </summary>
        /// <param name="modelSnapshotNamespace"> The model snapshot's namespace. </param>
        /// <param name="contextType"> The model snapshot's <see cref="DbContext" /> type. </param>
        /// <param name="modelSnapshotName"> The model snapshot's name. </param>
        /// <param name="model"> The model. </param>
        /// <returns> The model snapshot code. </returns>
        string GenerateSnapshot(
            [NotNull] string modelSnapshotNamespace,
            [NotNull] Type contextType,
            [NotNull] string modelSnapshotName,
            [NotNull] IModel model);

        /// <summary>
        ///     Gets the file extension code files should use.
        /// </summary>
        /// <value> The file extension. </value>
        string FileExtension { get; }
    }
}
