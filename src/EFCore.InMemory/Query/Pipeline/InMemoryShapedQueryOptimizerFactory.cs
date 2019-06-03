// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryShapedQueryOptimizerFactory : ShapedQueryOptimizerFactory
    {
        public override ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new InMemoryShapedQueryOptimizer();
        }
    }
}
