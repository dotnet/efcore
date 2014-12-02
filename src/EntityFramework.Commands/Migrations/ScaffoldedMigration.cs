// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class ScaffoldedMigration
    {
        private readonly string _migrationId;
        private string _directory;
        private string _modelSnapshotDirectory;

        public ScaffoldedMigration([NotNull] string migrationId)
        {
            Check.NotNull(migrationId, "migrationId");

            _migrationId = migrationId;
        }

        public virtual string MigrationId
        {
            get { return _migrationId; }
        }

        public virtual string SnapshotModelClass { get;[param: CanBeNull] set; }
        public virtual string Language { get;[param: CanBeNull]  set; }

        public virtual string MigrationCode { get;[param: CanBeNull] set; }
        public virtual string MigrationMetadataCode { get;[param: CanBeNull] set; }
        public virtual string SnapshotModelCode { get;[param: CanBeNull] set; }

        public virtual string Directory
        {
            get { return _directory; }
            [param: CanBeNull]
            set
            {
                Check.NotEmpty(value, "value");

                _directory = value;
            }
        }

        public virtual string ModelSnapshotDirectory
        {
            get { return _modelSnapshotDirectory; }
            [param: CanBeNull]
            set
            {
                Check.NotEmpty(value, "value");

                _modelSnapshotDirectory = value;
            }
        }
    }
}
