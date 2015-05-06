// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreServices : DataStoreServices
    {
        public InMemoryDataStoreServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDataStore Store => GetService<IInMemoryDataStore>();
        public override IQueryContextFactory QueryContextFactory => GetService<InMemoryQueryContextFactory>();
        public override IDataStoreCreator Creator => GetService<InMemoryDataStoreCreator>();
        public override IDataStoreConnection Connection => GetService<InMemoryConnection>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<InMemoryValueGeneratorSelector>();
        public override IDatabaseFactory DatabaseFactory => GetService<InMemoryDatabaseFactory>();
        public override IModelSource ModelSource => GetService<InMemoryModelSource>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<InMemoryValueGeneratorCache>();
    }
}
