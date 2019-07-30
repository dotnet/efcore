// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryShapedQueryOptimizerFactory : IShapedQueryOptimizerFactory
    {
        private readonly ShapedQueryOptimizerDependencies _dependencies;

        public InMemoryShapedQueryOptimizerFactory(ShapedQueryOptimizerDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public virtual ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new InMemoryShapedQueryOptimizer(_dependencies);
        }
    }
}
