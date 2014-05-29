// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity
{
    public static class AzureTableStorageDbContextExtensions
    {
        public static DbContextOptions UseAzureTableStorge([NotNull] this DbContextOptions options, [NotNull] string connectionString, bool batchRequests = false)
        {
            Check.NotNull(options, "options");
            Check.NotNull(connectionString, "connectionString");

            ((IDbContextOptionsExtensions)options).AddOrUpdateExtension<AzureTableStorageOptionsExtension>(
                e =>
                    {
                        e.ConnectionString = connectionString;
                        e.UseBatching = batchRequests;
                    });

            return options;
        }
    }
}
