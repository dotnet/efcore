// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalQueryOptimizerFactory : IQueryOptimizerFactory
    {
        private readonly QueryOptimizerDependencies _dependencies;
        private readonly RelationalQueryOptimizerDependencies _relationalDependencies;

        public RelationalQueryOptimizerFactory(
            QueryOptimizerDependencies dependencies,
            RelationalQueryOptimizerDependencies relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual QueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new RelationalQueryOptimizer(
                _dependencies,
                _relationalDependencies,
                queryCompilationContext);
        }
    }
}
