// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class MigrationMetadata : IMigrationMetadata
    {
        private readonly string _name;
        private readonly string _timestamp;

        public MigrationMetadata([NotNull] string name, [NotNull] string timestamp)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(timestamp, "timestamp");

            _name = name;
            _timestamp = timestamp;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string Timestamp
        {
            get { return _timestamp; }
        }

        public virtual IModel TargetModel { get; [param: NotNull] set; }

        public virtual IReadOnlyList<MigrationOperation> UpgradeOperations { get; [param: NotNull] set; }

        public virtual IReadOnlyList<MigrationOperation> DowngradeOperations { get; [param: NotNull] set; }
    }
}
