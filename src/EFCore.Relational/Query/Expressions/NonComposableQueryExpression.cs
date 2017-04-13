// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Reducible annotation expression representing a relational query expression
    ///     that cannot be composed with other relational query expressions.
    /// </summary>
    public sealed class NonComposableQueryExpression : Expression
    {
        /// <summary>
        ///     Creates an instance of <see cref="NonComposableQueryExpression"/>.
        /// </summary>
        /// <param name="queryExpression"> The query expression. </param>
        public NonComposableQueryExpression([NotNull] Expression queryExpression)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));

            QueryExpression = queryExpression;
        }

        /// <summary>
        ///     The query expression.
        /// </summary>
        public Expression QueryExpression { get; }

        /// <summary>
        ///     The type.
        /// </summary>
        public override Type Type => QueryExpression.Type;

        /// <summary>
        ///     Type of the node.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced
        ///     form.
        /// </summary>
        /// <returns>True if the node can be reduced, otherwise false.</returns>
        public override bool CanReduce => true;

        /// <summary>
        ///     Reduces this node to a simpler expression. If CanReduce returns true, this should return a valid expression. This method can
        ///     return another node which itself must be reduced.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public override Expression Reduce() => QueryExpression;

        /// <summary>
        ///     Reduces the node and then calls the visitor delegate on the reduced expression. The method throws an exception if the node is not
        ///     reducible.
        /// </summary>
        /// <returns>The expression being visited, or an expression which should replace it in the tree.</returns>
        /// <param name="visitor">An instance of <see cref="T:System.Func`2" />.</param>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var queryExpression = visitor.Visit(QueryExpression);

            return queryExpression != QueryExpression
                ? new NonComposableQueryExpression(queryExpression)
                : this;
        }
    }
}
