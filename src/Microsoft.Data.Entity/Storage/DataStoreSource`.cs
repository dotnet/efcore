// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource<TDataStore, TConfiguration, TCreator, TConnection> : DataStoreSource
        where TDataStore : DataStore
        where TConfiguration : EntityConfigurationExtension
        where TCreator : DataStoreCreator
        where TConnection : DataStoreConnection
    {
        public override DataStore GetStore(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TDataStore>();
        }

        public override DataStoreCreator GetCreator(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TCreator>();
        }

        public override DataStoreConnection GetConnection(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TConnection>();
        }

        public override bool IsConfigured(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.EntityConfiguration.Extensions.OfType<TConfiguration>().Any();
        }
    }
}
