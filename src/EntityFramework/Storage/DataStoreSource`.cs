// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource<TDataStore, TConfiguration, TCreator, TConnection, TValueGeneratorCache, TDatabase, TModelBuilderSelector> : DataStoreSource
        where TDataStore : DataStore
        where TConfiguration : DbContextOptionsExtension
        where TCreator : DataStoreCreator
        where TConnection : DataStoreConnection
        where TValueGeneratorCache : ValueGeneratorCache
        where TDatabase : Database
        where TModelBuilderSelector : IModelBuilderFactory
    {
        public override DataStore GetStore(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.Services.ServiceProvider.GetService<TDataStore>();
        }

        public override Database GetDatabase(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.Services.ServiceProvider.GetService<TDatabase>();
        }

        public override DataStoreCreator GetCreator(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.Services.ServiceProvider.GetService<TCreator>();
        }

        public override DataStoreConnection GetConnection(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.Services.ServiceProvider.GetService<TConnection>();
        }

        public override ValueGeneratorCache GetValueGeneratorCache(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.Services.ServiceProvider.GetService<TValueGeneratorCache>();
        }

        public override IModelBuilderFactory GetModelBuilderFactory(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.Services.ServiceProvider.GetService<TModelBuilderSelector>();
        }

        public override bool IsConfigured(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.ContextOptions.Extensions.OfType<TConfiguration>().Any();
        }
    }
}
