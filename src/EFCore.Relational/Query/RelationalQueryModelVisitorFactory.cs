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
    ///         A factory for instances of <see cref="EntityQueryModelVisitor" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class RelationalQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public RelationalQueryModelVisitorFactory(
            [NotNull] EntityQueryModelVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitorDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="EntityQueryModelVisitorFactory" />
        /// </summary>
        protected virtual RelationalQueryModelVisitorDependencies RelationalDependencies { get; }

        /// <summary>
        ///     Creates a new EntityQueryModelVisitor.
        /// </summary>
        /// <param name="queryCompilationContext"> Compilation context for the query. </param>
        /// <param name="parentEntityQueryModelVisitor"> The visitor for the outer query. </param>
        /// <returns>
        ///     An EntityQueryModelVisitor.
        /// </returns>
        public override EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new RelationalQueryModelVisitor(
                Dependencies,
                RelationalDependencies,
                (RelationalQueryCompilationContext)Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);
    }
}
