// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class InMemoryDatabaseProviderServices : DatabaseProviderServices
    {
        public InMemoryDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IDatabase Database => GetService<IInMemoryDatabase>();
        public override IDbContextTransactionManager TransactionManager => GetService<InMemoryTransactionManager>();
        public override IQueryContextFactory QueryContextFactory => GetService<InMemoryQueryContextFactory>();
        public override IDatabaseCreator Creator => GetService<InMemoryDatabaseCreator>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<InMemoryValueGeneratorSelector>();
        public override IModelSource ModelSource => GetService<InMemoryModelSource>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<InMemoryValueGeneratorCache>();
        public override IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory => GetService<InMemoryEntityQueryableExpressionVisitorFactory>();
        public override IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory => GetService<InMemoryQueryModelVisitorFactory>();
    }
}
