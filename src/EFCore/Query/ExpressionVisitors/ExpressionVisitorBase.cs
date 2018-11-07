// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A base expression visitor that ignores Block expressions.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class ExpressionVisitorBase : RelinqExpressionVisitor
    {
        /// <summary>
        ///     Visits the children of the extension expression.
        /// </summary>
        /// <returns>
        ///     The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="extensionExpression">The expression to visit.</param>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            return extensionExpression is NullConditionalExpression ? extensionExpression : base.VisitExtension(extensionExpression);
        }

        /// <summary>
        ///     Visits the children of the subquery expression.
        /// </summary>
        /// <returns>
        ///     The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="subQueryExpression">The expression to visit.</param>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            subQueryExpression.QueryModel.TransformExpressions(Visit);

            return base.VisitSubQuery(subQueryExpression);
        }

        /// <summary>Visits the children of the <see cref="T:System.Linq.Expressions.Expression`1" />.</summary>
        /// <returns>
        ///     The modified expression, if it or any subexpression was modified; otherwise, returns the original
        ///     expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var newBody = Visit(node.Body);

            return newBody == node.Body
                ? node
                : Expression.Lambda(newBody, node.Name, node.TailCall, node.Parameters);
        }
    }
}
