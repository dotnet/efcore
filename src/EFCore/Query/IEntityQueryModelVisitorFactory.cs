// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A factory for creating EntityQueryModelVisitors.
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
