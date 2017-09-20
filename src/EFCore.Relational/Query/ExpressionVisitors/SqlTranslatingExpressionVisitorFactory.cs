// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A factory for creating instances of <see cref="SqlTranslatingExpressionVisitor" />.
    /// </summary>
    public class SqlTranslatingExpressionVisitorFactory : ISqlTranslatingExpressionVisitorFactory
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlTranslatingExpressionVisitorFactory" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public SqlTranslatingExpressionVisitorFactory([NotNull] SqlTranslatingExpressionVisitorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="SqlTranslatingExpressionVisitorFactory" />
        /// </summary>
        protected virtual SqlTranslatingExpressionVisitorDependencies Dependencies { get; }

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
        public virtual SqlTranslatingExpressionVisitor Create(
            RelationalQueryModelVisitor queryModelVisitor,
            SelectExpression targetSelectExpression = null,
            Expression topLevelPredicate = null,
            bool inProjection = false)
            => new SqlTranslatingExpressionVisitor(
                Dependencies,
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                targetSelectExpression,
                topLevelPredicate,
                inProjection);
    }
}
