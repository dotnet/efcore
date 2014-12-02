// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class InMemoryEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddInMemoryStore([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddScoped<DataStoreSource, InMemoryDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<InMemoryValueGeneratorCache>()
                    .AddSingleton<InMemoryValueGeneratorSelector>()
                    .AddSingleton<SimpleValueGeneratorFactory<InMemoryValueGenerator>>()
                    .AddSingleton<InMemoryDatabase>()
                    .AddScoped<InMemoryDataStoreServices>()
                    .AddScoped<InMemoryDatabaseFacade>()
                    .AddScoped<InMemoryDataStore>()
                    .AddScoped<InMemoryConnection>()
                    .AddScoped<InMemoryDataStoreCreator>());

            return builder;
        }
    }
}
