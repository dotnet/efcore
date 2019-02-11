// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     <para>
    ///         A factory for creating entity queryable expression visitors.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IEntityQueryableExpressionVisitorFactory
    {
        /// <summary>
        ///     Creates a new entity queryable ExpressionVisitor.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The query model visitor. </param>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     An ExpressionVisitor.
        /// </returns>
        ExpressionVisitor Create(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [CanBeNull] IQuerySource querySource);
    }
}
