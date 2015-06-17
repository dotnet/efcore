// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class InMemoryEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddInMemoryStore([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            ((IAccessor<IServiceCollection>)builder).Service
                .AddDataStoreSource<InMemoryDataStoreSource>(nameof(AddInMemoryStore))
                .TryAdd(new ServiceCollection()
                    .AddSingleton<InMemoryValueGeneratorCache>()
                    .AddSingleton<IInMemoryDatabase, InMemoryDatabase>()
                    .AddSingleton<InMemoryModelSource>()
                    .AddScoped<InMemoryValueGeneratorSelector>()
                    .AddScoped<InMemoryQueryContextFactory>()
                    .AddScoped<InMemoryDataStoreServices>()
                    .AddScoped<InMemoryDatabaseFactory>()
                    .AddScoped<IInMemoryDataStore, InMemoryDataStore>()
                    .AddScoped<InMemoryConnection>()
                    .AddScoped<InMemoryDataStoreCreator>());

            return builder;
        }
    }
}
