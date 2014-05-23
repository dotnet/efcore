// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public static class AzureTableStorageEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddAzureTableStorage(this EntityServicesBuilder builder, bool batching = false)
        {
            builder.ServiceCollection
                .AddSingleton<DataStoreSource, AzureTableStorageDataStoreSource>()
                .AddScoped<AzureTableStorageConnection, AzureTableStorageConnection>()
                .AddScoped<AzureTableStorageDataStoreCreator, AzureTableStorageDataStoreCreator>();
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
