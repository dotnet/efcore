// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

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
    /// </summary>
    public abstract class EntityQueryModelVisitorFactory : IEntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
