// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A relational factory for instances of <see cref="QueryCompilationContext" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class RelationalQueryCompilationContextFactory : QueryCompilationContextFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryCompilationContextFactory(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] RelationalQueryCompilationContextDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            relationalDependencies
                .NodeTypeProviderFactory
                .RegisterMethods(FromSqlExpressionNode.SupportedMethods, typeof(FromSqlExpressionNode));
        }

        /// <summary>
        ///     Creates a new QueryCompilationContext.
        /// </summary>
        /// <param name="async"> true if the query is asynchronous. </param>
        /// <returns>
        ///     A QueryCompilationContext.
        /// </returns>
        public override QueryCompilationContext Create(bool async)
            => async
                ? new RelationalQueryCompilationContext(
                    Dependencies,
                    new AsyncLinqOperatorProvider(),
                    new AsyncQueryMethodProvider(),
                    TrackQueryResults)
                : new RelationalQueryCompilationContext(
                    Dependencies,
                    new LinqOperatorProvider(),
                    new QueryMethodProvider(),
                    TrackQueryResults);
    }
}
