// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class InMemoryEntityServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddInMemoryStore([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            ((IAccessor<IServiceCollection>)builder).Service
                .AddSingleton<IDataStoreSource, InMemoryDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<IInMemoryModelBuilderFactory, InMemoryModelBuilderFactory>()
                    .AddSingleton<IInMemoryValueGeneratorCache, InMemoryValueGeneratorCache>()
                    .AddSingleton<IInMemoryDatabase, InMemoryDatabase>()
                    .AddSingleton<IInMemoryModelSource, InMemoryModelSource>()
                    .AddScoped<IInMemoryQueryContextFactory, InMemoryQueryContextFactory>()
                    .AddScoped<IInMemoryValueGeneratorSelector, InMemoryValueGeneratorSelector>()
                    .AddScoped<IInMemoryDataStoreServices, InMemoryDataStoreServices>()
                    .AddScoped<IInMemoryDatabaseFactory, InMemoryDatabaseFactory>()
                    .AddScoped<IInMemoryDataStore, InMemoryDataStore>()
                    .AddScoped<IInMemoryConnection, InMemoryConnection>()
                    .AddScoped<IInMemoryDataStoreCreator, InMemoryDataStoreCreator>());

            return builder;
        }
    }
}
