// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="QueryTranslationPreprocessor"/> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class QueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
    {
        private readonly QueryTranslationPreprocessorDependencies _dependencies;

        public QueryTranslationPreprocessorFactory(QueryTranslationPreprocessorDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public virtual QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
            => new QueryTranslationPreprocessor(_dependencies, queryCompilationContext);
    }
}
