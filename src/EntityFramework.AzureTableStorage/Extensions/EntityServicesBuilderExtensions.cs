// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.AzureTableStorage;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class EntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddAzureTableStorage([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddSingleton<AtsQueryFactory>()
                .AddSingleton<TableEntityAdapterFactory>()
                .AddSingleton<AtsValueReaderFactory>()
                .AddSingleton<AtsModelBuilderFactory>()
                .AddSingleton<AtsValueGeneratorCache>()
                .AddScoped<DataStoreSource, AtsDataStoreSource>()
                .AddScoped<AtsDataStoreServices>()
                .AddScoped<AtsDatabase>()
                .AddScoped<AtsDataStore>()
                .AddScoped<AtsConnection>()
                .AddScoped<AtsDataStoreCreator>()
                .AddScoped<AtsValueGeneratorCache>();

            return builder;
        }
    }
}
