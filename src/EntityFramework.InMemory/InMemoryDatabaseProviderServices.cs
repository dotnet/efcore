// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity
{
    public class InMemoryDatabaseProviderServices : DatabaseProviderServices
    {
        public InMemoryDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IDatabase Database => GetService<IInMemoryDatabase>();
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<InMemoryQueryCompilationContextFactory>();
        public override IQueryContextFactory QueryContextFactory => GetService<InMemoryQueryContextFactory>();
        public override IDatabaseCreator Creator => GetService<InMemoryDatabaseCreator>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<InMemoryValueGeneratorSelector>();
        public override IModelSource ModelSource => GetService<InMemoryModelSource>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<InMemoryValueGeneratorCache>();
    }
}
