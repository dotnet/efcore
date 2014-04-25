// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public interface IMigrationMetadata
    {
        string Name { get; }
        string Timestamp { get; }
        IModel TargetModel { get; }
        IReadOnlyList<MigrationOperation> UpgradeOperations { get; }
        IReadOnlyList<MigrationOperation> DowngradeOperations { get; }
    }
}
