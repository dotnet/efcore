// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class ScaffoldedMigration
    {
        public ScaffoldedMigration([NotNull] string migrationId)
        {
            Check.NotNull(migrationId, "migrationId");

            MigrationId = migrationId;
        }

        public virtual string MigrationId { get; }

        public virtual string SnapshotModelClass { get;[param: NotNull] set; }
        public virtual string Language { get;[param: NotNull]  set; }

        public virtual string MigrationCode { get;[param: NotNull] set; }
        public virtual string MigrationMetadataCode { get;[param: NotNull] set; }
        public virtual string SnapshotModelCode { get;[param: NotNull] set; }

        public virtual string MigrationNamespace { get;[param: NotNull]set; }

        public virtual string ModelSnapshotNamespace { get;[param: NotNull]set; }

        public virtual Migration LastMigration { get;[param: CanBeNull] set; }
        public virtual ModelSnapshot LastModelSnapshot { get;[param: CanBeNull] set; }
    }
}
