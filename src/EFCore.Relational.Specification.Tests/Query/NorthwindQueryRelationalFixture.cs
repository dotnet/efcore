// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindQueryRelationalFixture<TModelCustomizer> : NorthwindQueryFixtureBase<TModelCustomizer>
        where TModelCustomizer : IModelCustomizer, new()
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        public new IRelationalTestStore<DbConnection> TestStore => (IRelationalTestStore<DbConnection>)base.TestStore;

        protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c
                .Log(RelationalEventId.QueryClientEvaluationWarning)
                .Log(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning)
                .Log(RelationalEventId.QueryPossibleExceptionWithAggregateOperator));

        protected override Type ContextType => typeof(NorthwindRelationalContext);
    }
}
