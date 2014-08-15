// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Utilities;

namespace Microsoft.Data.Entity.Design
{
    public class ScaffoldedMigration
    {
        private readonly string _migrationId;

        public ScaffoldedMigration([NotNull] string migrationId)
        {
            Check.NotNull(migrationId, "migrationId");

            _migrationId = migrationId;
        }

        public virtual string MigrationId
        {
            get { return _migrationId; }
        }

        public virtual string MigrationNamespace { get; [param: CanBeNull] set; }
        public virtual string MigrationClass { get; [param: CanBeNull] set; }
        public virtual string SnapshotModelClass { get; [param: CanBeNull] set; }

        public virtual string MigrationCode { get; [param: CanBeNull] set; }
        public virtual string MigrationMetadataCode { get; [param: CanBeNull] set; }
        public virtual string SnapshotModelCode { get; [param: CanBeNull] set; }

        public virtual string MigrationFile { get; [param: CanBeNull] set; }
        public virtual string MigrationMetadataFile { get; [param: CanBeNull] set; }
        public virtual string SnapshotModelFile { get; [param: CanBeNull] set; }
    }
}
