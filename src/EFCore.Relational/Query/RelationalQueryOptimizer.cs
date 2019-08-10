// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    }
}
