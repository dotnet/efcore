// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     <para>
    ///         Factory for creating instances of <see cref="SqlTranslatingExpressionVisitor" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public interface ISqlTranslatingExpressionVisitorFactory
    {
        /// <summary>
        ///     Creates a new SqlTranslatingExpressionVisitor.
        /// </summary>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="targetSelectExpression"> The target select expression. </param>
        /// <param name="topLevelPredicate"> The top level predicate. </param>
        /// <param name="inProjection"> true if we are translating a projection. </param>
        /// <returns>
        ///     A SqlTranslatingExpressionVisitor.
        /// </returns>
        SqlTranslatingExpressionVisitor Create(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false);
    }
}
