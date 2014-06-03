// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    internal class AzureTableStorageDataStoreSource : DataStoreSource<
        AzureTableStorageDataStore,
        AzureTableStorageOptionsExtension,
        AzureTableStorageDataStoreCreator,
        AzureTableStorageConnection,
        AzureTableStorageValueGeneratorCache,
        AtsDatabase>
    {
        public override bool IsAvailable(DbContextConfiguration configuration)
        {
            return IsConfigured(configuration);
        }

        public override string Name
        {
            get { return "AzureTableStorage"; }
        }
    }
}
