// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public static class InMemoryEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddInMemoryStore([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddScoped<InMemoryDataStore, InMemoryDataStore>()
                .AddSingleton<DataStoreSource, InMemoryDataStoreSource>()
                // TODO: Need to be able to pick the appropriate identity generator for the data store in use
                .AddSingleton<IdentityGeneratorFactory, InMemoryIdentityGeneratorFactory>();

            return builder;
        }
    }
}
