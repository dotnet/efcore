// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class ScaffoldedMigration
    {
        public ScaffoldedMigration(
            [NotNull] string language,
            [CanBeNull] string lastMigrationId,
            [NotNull] string migrationCode,
            [NotNull] string migrationId,
            [NotNull] string migrationMetadataCode,
            [NotNull] string migrationSubnamespace,
            [NotNull] string modelSnapshotCode,
            [NotNull] string modelSnapshotName,
            [NotNull] string modelSnapshotSubnamespace)
        {
            Check.NotEmpty(language, nameof(language));
            Check.NotEmpty(migrationCode, nameof(migrationCode));
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotEmpty(migrationMetadataCode, nameof(migrationMetadataCode));
            Check.NotNull(migrationSubnamespace, nameof(migrationSubnamespace));
            Check.NotEmpty(modelSnapshotCode, nameof(modelSnapshotCode));
            Check.NotEmpty(modelSnapshotName, nameof(modelSnapshotName));
            Check.NotNull(modelSnapshotSubnamespace, nameof(modelSnapshotSubnamespace));

            Language = language;
            LastMigrationId = lastMigrationId;
            MigrationCode = migrationCode;
            MigrationId = migrationId;
            MigrationMetadataCode = migrationMetadataCode;
            MigrationSubnamespace = migrationSubnamespace;
            ModelSnapshotCode = modelSnapshotCode;
            ModelSnapshotName = modelSnapshotName;
            ModelSnapshotSubnamespace = modelSnapshotSubnamespace;
        }

        public virtual string Language { get; }
        public virtual string LastMigrationId { get; }
        public virtual string MigrationCode { get; }
        public virtual string MigrationId { get; }
        public virtual string MigrationMetadataCode { get; }
        public virtual string MigrationSubnamespace { get; }
        public virtual string ModelSnapshotCode { get; }
        public virtual string ModelSnapshotName { get; }
        public virtual string ModelSnapshotSubnamespace { get; }
    }
}
