// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public static class AzureTableStorageEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddAzureTableStorage([NotNull] this EntityServicesBuilder builder, bool batching = false)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddSingleton<DataStoreSource, AzureTableStorageDataStoreSource>()
                .AddSingleton<AzureTableStorageQueryFactory>()
                .AddSingleton<TableEntityAdapterFactory>()
                .AddSingleton<AtsValueReaderFactory>()
                .AddSingleton<TableFilterFactory>()
                .AddScoped<AtsDatabase>()
                .AddScoped<AzureTableStorageConnection>()
                .AddScoped<AzureTableStorageDataStoreCreator>()
                .AddScoped<AzureTableStorageValueGeneratorCache>();

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
