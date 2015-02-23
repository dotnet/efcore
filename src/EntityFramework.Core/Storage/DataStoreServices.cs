// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreServices
    {
        public abstract DataStore Store { get; }
        public abstract DataStoreCreator Creator { get; }
        public abstract DataStoreConnection Connection { get; }
        public abstract IValueGeneratorSelector ValueGeneratorSelector { get; }
        public abstract Database Database { get; }
        public abstract ModelBuilderFactory ModelBuilderFactory { get; }
        public abstract ModelSource ModelSource { get; }
        public abstract QueryContextFactory QueryContextFactory { get; }

        public static Func<IServiceProvider, DataStore> DataStoreFactory => p => GetStoreServices(p).Store;

        public static Func<IServiceProvider, QueryContextFactory> QueryContextFactoryFactory => p => GetStoreServices(p).QueryContextFactory;

        public static Func<IServiceProvider, Database> DatabaseFactory => p => GetStoreServices(p).Database;

        public static Func<IServiceProvider, DataStoreCreator> DataStoreCreatorFactory => p => GetStoreServices(p).Creator;

        public static Func<IServiceProvider, IValueGeneratorSelector> ValueGeneratorSelectorFactory => p => GetStoreServices(p).ValueGeneratorSelector;

        public static Func<IServiceProvider, DataStoreConnection> ConnectionFactory => p => GetStoreServices(p).Connection;

        public static Func<IServiceProvider, ModelBuilderFactory> ModelBuilderFactoryFactory => p => GetStoreServices(p).ModelBuilderFactory;

        private static DataStoreServices GetStoreServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredServiceChecked<DbContextServices>().DataStoreServices;
    }
}
