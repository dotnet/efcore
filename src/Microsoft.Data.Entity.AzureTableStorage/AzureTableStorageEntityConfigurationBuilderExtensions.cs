// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.AzureTableStorage;

namespace Microsoft.Data.Entity
{
    public static class AzureTableStorageEntityConfigurationBuilderExtensions
    {
        public static DbContextOptions UseAzureTableStorge(this DbContextOptions builder, string connectionString)
        {
            builder.AddBuildAction(c => c.AddOrUpdateExtension<AzureTableStorageConfigurationExtension>(
                e => e.ConnectionString = connectionString));

            return builder;
        }
    }
}
