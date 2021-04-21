// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Represents the file generated for a migration.
    /// </summary>
    public class MigrationFiles
    {
        /// <summary>
        ///     Gets or sets the path to the migration file.
        /// </summary>
        /// <value> The path to the migration file. </value>
        public virtual string? MigrationFile { get; set; }

        /// <summary>
        ///     Gets or sets the path to the migration metadata file.
        /// </summary>
        /// <value> The path to the migration metadata file. </value>
        public virtual string? MetadataFile { get; set; }

        /// <summary>
        ///     Gets or sets the path to the model snapshot file.
        /// </summary>
        /// <value> The path to the model snapshot file. </value>
        public virtual string? SnapshotFile { get; set; }
    }
}
