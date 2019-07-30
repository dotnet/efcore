// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ShapedQueryOptimizer
    {
        public ShapedQueryOptimizer(ShapedQueryOptimizerDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        protected virtual ShapedQueryOptimizerDependencies Dependencies { get; }

        public virtual Expression Visit(Expression query)
        {
            return query;
        }
    }
}
