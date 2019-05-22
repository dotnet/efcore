// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A factory for <see cref="QueryContext" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        /// <summary>
        ///     Creates a new <see cref="QueryContextFactory"/> instance using the given dependencies.
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        protected QueryContextFactory([NotNull] QueryContextDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual QueryContextDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a query buffer.
        /// </summary>
        /// <returns>
        ///     The new query buffer.
        /// </returns>
        protected virtual IQueryBuffer CreateQueryBuffer()
            => new QueryBuffer(Dependencies);

        /// <summary>
        ///     Creates a new QueryContext.
        /// </summary>
        /// <returns>
        ///     A QueryContext.
        /// </returns>
        public abstract QueryContext Create();
    }
}
