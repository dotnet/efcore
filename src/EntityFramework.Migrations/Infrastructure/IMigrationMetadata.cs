// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public interface IMigrationMetadata
    {
        string MigrationId { get; }
        string ProductVersion { get; }
        IModel TargetModel { get; }
        IReadOnlyList<MigrationOperation> UpgradeOperations { get; }
        IReadOnlyList<MigrationOperation> DowngradeOperations { get; }
    }
}
