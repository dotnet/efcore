// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class ScaffoldedMigration
    {
        public ScaffoldedMigration(
            [NotNull] string fileExtension,
            [CanBeNull] string previousMigrationId,
            [NotNull] string migrationCode,
            [NotNull] string migrationId,
            [NotNull] string metadataCode,
            [NotNull] string migrationSubNamespace,
            [NotNull] string snapshotCode,
            [NotNull] string snapshotName,
            [NotNull] string snapshotSubNamespace)
        {
            Check.NotEmpty(fileExtension, nameof(fileExtension));
            Check.NotEmpty(migrationCode, nameof(migrationCode));
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotEmpty(metadataCode, nameof(metadataCode));
            Check.NotNull(migrationSubNamespace, nameof(migrationSubNamespace));
            Check.NotEmpty(snapshotCode, nameof(snapshotCode));
            Check.NotEmpty(snapshotName, nameof(snapshotName));
            Check.NotNull(snapshotSubNamespace, nameof(snapshotSubNamespace));

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

        public virtual string FileExtension { get; }
        public virtual string PreviousMigrationId { get; }
        public virtual string MigrationCode { get; }
        public virtual string MigrationId { get; }
        public virtual string MetadataCode { get; }
        public virtual string MigrationSubNamespace { get; }
        public virtual string SnapshotCode { get; }
        public virtual string SnapshotName { get; }
        public virtual string SnapshotSubnamespace { get; }
    }
}
