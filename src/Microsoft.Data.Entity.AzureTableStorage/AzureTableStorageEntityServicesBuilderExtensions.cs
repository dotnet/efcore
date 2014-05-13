// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public static class AzureTableStorageEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddAzureTableStorage(this EntityServicesBuilder builder)
        {
            builder.ServiceCollection
                .AddSingleton<DataStoreSource, AzureTableStorageDataStoreSource>()
                .AddScoped<AzureStorageDataStore, AzureStorageDataStore>()
                .AddScoped<AzureTableStorageConnection, AzureTableStorageConnection>()
                .AddScoped<AzureTableStorageDataStoreCreator, AzureTableStorageDataStoreCreator>();

            return builder;
        }
    }
}
