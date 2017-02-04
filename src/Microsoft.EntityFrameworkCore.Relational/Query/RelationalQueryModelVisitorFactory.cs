// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A factory for instances of <see cref="EntityQueryModelVisitor" />.
    /// </summary>
    public class RelationalQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
