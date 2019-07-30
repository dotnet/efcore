// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryShapedQueryOptimizer : ShapedQueryOptimizer
    {
        public InMemoryShapedQueryOptimizer(ShapedQueryOptimizerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override Expression Visit(Expression query)
        {
            query = base.Visit(query);

            return query;
        }
    }
}
