// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource<TDataStore, TConfiguration, TCreator, TConnection> : DataStoreSource
        where TDataStore : DataStore
        where TConfiguration : EntityConfigurationExtension
        where TCreator : DataStoreCreator
        where TConnection : DataStoreConnection
    {
        public override DataStore GetStore(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TDataStore>();
        }

        public override DataStoreCreator GetCreator(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TCreator>();
        }

        public override DataStoreConnection GetConnection(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TConnection>();
        }

        public override bool IsConfigured(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.ContextOptions.Extensions.OfType<TConfiguration>().Any();
        }
    }
}
