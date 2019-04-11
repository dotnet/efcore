// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Creates instances of <see cref="EntityQueryModelVisitor" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    [EntityFrameworkInternal]
    public abstract class EntityQueryModelVisitorFactory : IEntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected EntityQueryModelVisitorFactory(
            [NotNull] EntityQueryModelVisitorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="EntityQueryModelVisitorFactory" />
        /// </summary>
        protected virtual EntityQueryModelVisitorDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a new <see cref="EntityQueryModelVisitor" />.
        /// </summary>
        /// <param name="queryCompilationContext">
        ///     Compilation context for the query.
        /// </param>
        /// <param name="parentEntityQueryModelVisitor">
        ///     The visitor for the outer query.
        /// </param>
        /// <returns> The new created visitor. </returns>
        public abstract EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
