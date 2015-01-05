// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public interface IMigrationMetadata
    {
        string MigrationId { get; }
        string ProductVersion { get; }
        IModel TargetModel { get; }
    }
}
