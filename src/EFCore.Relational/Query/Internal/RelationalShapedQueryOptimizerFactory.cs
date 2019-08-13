// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="ShapedQueryOptimizer"/> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class RelationalShapedQueryOptimizerFactory : IShapedQueryOptimizerFactory
    {
        private readonly ShapedQueryOptimizerDependencies _dependencies;
        private readonly RelationalShapedQueryOptimizerDependencies _relationalDependencies;

        public RelationalShapedQueryOptimizerFactory(
            ShapedQueryOptimizerDependencies dependencies,
            RelationalShapedQueryOptimizerDependencies relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual ShapedQueryOptimizer Create(QueryCompilationContext queryCompilationContext)
        {
            return new RelationalShapedQueryOptimizer(
                _dependencies,
                _relationalDependencies,
                queryCompilationContext);
        }
    }
}
