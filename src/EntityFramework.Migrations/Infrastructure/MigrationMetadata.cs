// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class MigrationMetadata : IMigrationMetadata
    {
        private readonly string _migrationId;
        private readonly Type _contextType;

        public MigrationMetadata([NotNull] string migrationId, [NotNull] Type contextType)
        {
            Check.NotEmpty(migrationId, "migrationId");
            Check.NotNull(contextType, "contextType");

            if (!MigrationMetadataExtensions.IsValidMigrationId(migrationId))
            {
                throw new ArgumentException(Strings.FormatInvalidMigrationId(migrationId));
            }

            _migrationId = migrationId;
            _contextType = contextType;
        }

        public virtual string MigrationId
        {
            get { return _migrationId; }
        }

        public virtual Type ContextType
        {
            get { return _contextType; }
        }

        public virtual IModel TargetModel { get; [param: NotNull] set; }

        public virtual IReadOnlyList<MigrationOperation> UpgradeOperations { get; [param: NotNull] set; }

        public virtual IReadOnlyList<MigrationOperation> DowngradeOperations { get; [param: NotNull] set; }
    }
}
