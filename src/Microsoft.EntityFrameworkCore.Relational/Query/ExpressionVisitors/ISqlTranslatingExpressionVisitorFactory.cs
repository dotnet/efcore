// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     Factory for creating instances of <see cref="SqlTranslatingExpressionVisitor" />.
    /// </summary>
    public interface ISqlTranslatingExpressionVisitorFactory
    {
        /// <summary>
        ///     Creates a new SqlTranslatingExpressionVisitor.
        /// </summary>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="topLevelPredicate"> The top level predicate. </param>
        /// <param name="inProjection"> true if we are translating a projection. </param>
        /// <param name="addToProjections"> false to avoid adding columns to projections. </param>
        /// <returns>
        ///     A SqlTranslatingExpressionVisitor.
        /// </returns>
        SqlTranslatingExpressionVisitor Create(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] Expression topLevelPredicate = null,
            bool inProjection = false,
            bool addToProjections = true);
    }
}
