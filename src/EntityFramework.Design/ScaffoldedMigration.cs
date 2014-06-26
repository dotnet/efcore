// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design
{
    public class ScaffoldedMigration
    {
        public string MigrationNamespace { get; set; }
        public string MigrationClass { get; set; }
        public string SnapshotModelClass { get; set; }

        public string MigrationCode { get; set; }
        public string MigrationMetadataCode { get; set; }
        public string SnapshotModelCode { get; set; }

        public string MigrationFile { get; set; }
        public string MigrationMetadataFile { get; set; }
        public string SnapshotModelFile { get; set; }
    }
}
