// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreServices
    {
        public abstract DataStore Store { get; }
        public abstract DataStoreCreator Creator { get; }
        public abstract DataStoreConnection Connection { get; }
        public abstract ValueGeneratorCache ValueGeneratorCache { get; }
        public abstract Database Database { get; }
        public abstract ModelBuilderFactory ModelBuilderFactory { get; }
        public abstract ModelSource ModelSource { get; }

        public static Func<IServiceProvider, DbContextService<DataStoreServices>> DataStoreServicesFactory
        {
            get { return p => new DbContextService<DataStoreServices>(() => GetStoreServices(p)); }
        }

        public static Func<IServiceProvider, DbContextService<DataStore>> DataStoreFactory
        {
            get { return p => new DbContextService<DataStore>(() => GetStoreServices(p).Store); }
        }

        public static Func<IServiceProvider, DbContextService<Database>> DatabaseFactory
        {
            get { return p => new DbContextService<Database>(() => GetStoreServices(p).Database); }
        }

        public static Func<IServiceProvider, DbContextService<DataStoreCreator>> DataStoreCreatorFactory
        {
            get { return p => new DbContextService<DataStoreCreator>(() => GetStoreServices(p).Creator); }
        }

        public static Func<IServiceProvider, DbContextService<ValueGeneratorCache>> ValueGeneratorCacheFactory
        {
            get { return p => new DbContextService<ValueGeneratorCache>(() => GetStoreServices(p).ValueGeneratorCache); }
        }

        public static Func<IServiceProvider, DbContextService<DataStoreConnection>> ConnectionFactory
        {
            get { return p => new DbContextService<DataStoreConnection>(() => GetStoreServices(p).Connection); }
        }

        public static Func<IServiceProvider, DbContextService<ModelBuilderFactory>> ModelBuilderFactoryFactory
        {
            get { return p => new DbContextService<ModelBuilderFactory>(() => GetStoreServices(p).ModelBuilderFactory); }
        }

        protected static DataStoreServices GetStoreServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            return serviceProvider.GetRequiredServiceChecked<DbContextServices>().DataStoreServices;
        }
    }
}
