// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreServices : DataStoreServices
    {
        private readonly IServiceProvider _serviceProvider;

        public InMemoryDataStoreServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        public override DataStore Store => _serviceProvider.GetRequiredService<InMemoryDataStore>();

        public override QueryContextFactory QueryContextFactory => _serviceProvider.GetRequiredService<InMemoryQueryContextFactory>();

        public override DataStoreCreator Creator => _serviceProvider.GetRequiredService<InMemoryDataStoreCreator>();

        public override DataStoreConnection Connection => _serviceProvider.GetRequiredService<InMemoryConnection>();

        public override IValueGeneratorSelector ValueGeneratorSelector => _serviceProvider.GetRequiredService<InMemoryValueGeneratorSelector>();

        public override Database Database => _serviceProvider.GetRequiredService<InMemoryDatabaseFacade>();

        public override ModelBuilderFactory ModelBuilderFactory => _serviceProvider.GetRequiredService<InMemoryModelBuilderFactory>();

        public override ModelSource ModelSource => _serviceProvider.GetRequiredService<InMemoryModelSource>();
    }
}
