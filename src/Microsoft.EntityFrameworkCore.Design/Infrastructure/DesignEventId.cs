// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public enum DesignEventId
    {
        ForceRemoveMigration = 1,
        RemovingMigration,
        NoMigrationFile,
        NoMigrationMetadataFile,
        ManuallyDeleted,
        RemovingSnapshot,
        NoSnapshotFile,
        WritingSnapshot,
        ReusingNamespace,
        ReusingDirectory,
        RevertingSnapshot,
        WritingMigration,
        ReusingSnapshotName,
        DestructiveOperation,
        ForeignMigrations
    }
}
