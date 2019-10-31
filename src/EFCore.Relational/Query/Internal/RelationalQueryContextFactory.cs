// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         A factory for <see cref="RelationalQueryContext" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class RelationalQueryContextFactory : IQueryContextFactory
    {
        private readonly QueryContextDependencies _dependencies;
        private readonly RelationalQueryContextDependencies _relationalDependencies;

        /// <summary>
        ///     Creates a new <see cref="RelationalQueryContextFactory" /> instance using the given dependencies.
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        /// <param name="relationalDependencies"> Relational-specific dependencies. </param>
        public RelationalQueryContextFactory(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] RelationalQueryContextDependencies relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Creates a new <see cref="RelationalQueryContext" />.
        /// </summary>
        /// <returns>
        ///     A QueryContext.
        /// </returns>
        public virtual QueryContext Create()
            => new RelationalQueryContext(_dependencies, _relationalDependencies);
    }
}
