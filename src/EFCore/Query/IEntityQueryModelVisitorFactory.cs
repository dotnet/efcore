// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A factory for creating EntityQueryModelVisitors.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IEntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     Creates a new EntityQueryModelVisitor.
        /// </summary>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <param name="parentEntityQueryModelVisitor"> The parent entity query model visitor. </param>
        /// <returns>
        ///     An EntityQueryModelVisitor instance.
        /// </returns>
        EntityQueryModelVisitor Create(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
