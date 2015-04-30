// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreServices : IInMemoryDataStoreServices
    {
        private readonly IServiceProvider _serviceProvider;

        public InMemoryDataStoreServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        public virtual IDataStore Store => _serviceProvider.GetRequiredService<IInMemoryDataStore>();

        public virtual IQueryContextFactory QueryContextFactory => _serviceProvider.GetRequiredService<IInMemoryQueryContextFactory>();

        public virtual IDataStoreCreator Creator => _serviceProvider.GetRequiredService<IInMemoryDataStoreCreator>();

        public virtual IDataStoreConnection Connection => _serviceProvider.GetRequiredService<IInMemoryConnection>();

        public virtual IValueGeneratorSelector ValueGeneratorSelector => _serviceProvider.GetRequiredService<IInMemoryValueGeneratorSelector>();

        public virtual IDatabaseFactory DatabaseFactory => _serviceProvider.GetRequiredService<IInMemoryDatabaseFactory>();

        public virtual IModelBuilderFactory ModelBuilderFactory => _serviceProvider.GetRequiredService<IInMemoryModelBuilderFactory>();

        public virtual IModelSource ModelSource => _serviceProvider.GetRequiredService<IInMemoryModelSource>();
    }
}
