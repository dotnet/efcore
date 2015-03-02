// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public static class DataStoreServiceFactories
    {
        public static Func<IServiceProvider, IDataStore> DataStoreFactory => p => GetStoreServices(p).Store;

        public static Func<IServiceProvider, IQueryContextFactory> QueryContextFactoryFactory => p => GetStoreServices(p).QueryContextFactory;

        public static Func<IServiceProvider, IDatabaseFactory> DatabaseFactoryFactory => p => GetStoreServices(p).DatabaseFactory;

        public static Func<IServiceProvider, IDataStoreCreator> DataStoreCreatorFactory => p => GetStoreServices(p).Creator;

        public static Func<IServiceProvider, IValueGeneratorSelector> ValueGeneratorSelectorFactory => p => GetStoreServices(p).ValueGeneratorSelector;

        public static Func<IServiceProvider, IDataStoreConnection> ConnectionFactory => p => GetStoreServices(p).Connection;

        public static Func<IServiceProvider, IModelBuilderFactory> ModelBuilderFactoryFactory => p => GetStoreServices(p).ModelBuilderFactory;

        private static IDataStoreServices GetStoreServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredServiceChecked<DbContextServices>().DataStoreServices;
    }
}
