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
    public static class EntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddAzureTableStorage([NotNull] this EntityServicesBuilder builder, bool batching = false)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddSingleton<DataStoreSource, AtsDataStoreSource>()
                .AddSingleton<AtsQueryFactory>()
                .AddSingleton<TableEntityAdapterFactory>()
                .AddSingleton<AtsValueReaderFactory>()
                .AddSingleton<TableFilterFactory>()
                .AddScoped<AtsDatabase>()
                .AddScoped<AtsConnection>()
                .AddScoped<AtsDataStoreCreator>()
                .AddScoped<AtsValueGeneratorCache>();

            if (batching)
            {
                builder.ServiceCollection.AddScoped<AtsDataStore, AtsBatchedDataStore>();
            }
            else
            {
                builder.ServiceCollection.AddScoped<AtsDataStore, AtsDataStore>();
            }
            return builder;
        }
    }
}
