// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public static class AzureTableStorageEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddAzureTableStorage([NotNull] this EntityServicesBuilder builder, bool batching = false)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddSingleton<DataStoreSource, AzureTableStorageDataStoreSource>()
                .AddScoped<AzureTableStorageConnection, AzureTableStorageConnection>()
                .AddScoped<AzureTableStorageDataStoreCreator, AzureTableStorageDataStoreCreator>()
                .AddScoped<AzureTableStorageValueGeneratorCache, AzureTableStorageValueGeneratorCache>();
            if (batching)
            {
                builder.ServiceCollection.AddScoped<AzureTableStorageDataStore, AzureTableStorageBatchedDataStore>();
            }
            else
            {
                builder.ServiceCollection.AddScoped<AzureTableStorageDataStore, AzureTableStorageDataStore>();
            }
            return builder;
        }
    }
}
