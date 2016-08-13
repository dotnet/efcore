// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class MigrationFiles
    {
        public virtual string MigrationFile { get; [param: CanBeNull] set; }
        public virtual string MetadataFile { get; [param: CanBeNull] set; }
        public virtual string SnapshotFile { get; [param: CanBeNull] set; }
    }
}
