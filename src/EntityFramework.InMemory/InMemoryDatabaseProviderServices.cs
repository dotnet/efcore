// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDatabaseProviderServices : DatabaseProviderServices
    {
        public InMemoryDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDatabase Database => GetService<IInMemoryDatabase>();
        public override IQueryContextFactory QueryContextFactory => GetService<InMemoryQueryContextFactory>();
        public override IDatabaseCreator Creator => GetService<InMemoryDatabaseCreator>();
        public override IDatabaseConnection Connection => GetService<InMemoryConnection>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<InMemoryValueGeneratorSelector>();
        public override IModelSource ModelSource => GetService<InMemoryModelSource>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<InMemoryValueGeneratorCache>();
    }
}
