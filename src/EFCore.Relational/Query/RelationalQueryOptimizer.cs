// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalQueryOptimizer : QueryOptimizer
    {
        public RelationalQueryOptimizer(
            QueryOptimizerDependencies dependencies,
            RelationalQueryOptimizerDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            RelationalDependencies = relationalDependencies;
        }

        protected virtual RelationalQueryOptimizerDependencies RelationalDependencies { get; }

        public override Expression Visit(Expression query)
        {
            query = new FromSqlEntityQueryableInjectingExpressionVisitor().Visit(query);

            return base.Visit(query);
        }
    }
}
