// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="ShapedQueryCompilingExpressionVisitor" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IShapedQueryCompilingExpressionVisitorFactory
    {
        /// <summary>
        ///     Creates a new <see cref="ShapedQueryCompilingExpressionVisitor" /> for given <see cref="QueryCompilationContext" />.
        /// </summary>
        /// <param name="queryCompilationContext"> The query compilation context to use. </param>
        /// <returns> The created visitor. </returns>
        ShapedQueryCompilingExpressionVisitor Create([NotNull] QueryCompilationContext queryCompilationContext);
    }
}
