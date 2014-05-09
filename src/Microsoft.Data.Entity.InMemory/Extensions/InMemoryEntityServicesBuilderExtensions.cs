// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Framework.DependencyInjection
{
    public static class InMemoryEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddInMemoryStore([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                // TODO: Need to be able to pick the appropriate identity generator for the data store in use
                .AddSingleton<IdentityGeneratorFactory, InMemoryIdentityGeneratorFactory>()
                .AddSingleton<DataStoreSource, InMemoryDataStoreSource>()
                .AddSingleton<InMemoryDatabase, InMemoryDatabase>()
                .AddScoped<InMemoryDataStore, InMemoryDataStore>()
                .AddScoped<InMemoryConnection, InMemoryConnection>()
                .AddScoped<InMemoryDataStoreCreator, InMemoryDataStoreCreator>();

            return builder;
        }
    }
}
