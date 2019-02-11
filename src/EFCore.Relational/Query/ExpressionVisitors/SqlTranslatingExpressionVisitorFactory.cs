// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     <para>
    ///         A factory for creating instances of <see cref="SqlTranslatingExpressionVisitor" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
