// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class MigrationFiles
    {
        public virtual string MigrationFile { get; [param: CanBeNull] set; }
        public virtual string MigrationMetadataFile { get; [param: CanBeNull] set; }
        public virtual string ModelSnapshotFile { get; [param: CanBeNull] set; }
    }
}
