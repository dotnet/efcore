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

        public override IDataStore Store => Services.GetRequiredService<IInMemoryDataStore>();
        public override IQueryContextFactory QueryContextFactory => Services.GetRequiredService<InMemoryQueryContextFactory>();
        public override IDataStoreCreator Creator => Services.GetRequiredService<InMemoryDataStoreCreator>();
        public override IDataStoreConnection Connection => Services.GetRequiredService<InMemoryConnection>();
        public override IValueGeneratorSelector ValueGeneratorSelector => Services.GetRequiredService<InMemoryValueGeneratorSelector>();
        public override IDatabaseFactory DatabaseFactory => Services.GetRequiredService<InMemoryDatabaseFactory>();
        public override IModelSource ModelSource => Services.GetRequiredService<InMemoryModelSource>();
        public override IValueGeneratorCache ValueGeneratorCache => Services.GetRequiredService<InMemoryValueGeneratorCache>();
    }
}
