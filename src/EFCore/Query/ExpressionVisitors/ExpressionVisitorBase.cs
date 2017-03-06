// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
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
        ///     Visits the given node.
        /// </summary>
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression Visit([CanBeNull] Expression node)
            => node == null
               || node.NodeType == ExpressionType.Block
                ? node
                : base.Visit(node);

        /// <summary>
        ///     Visits the children of the extension expression.
        /// </summary>
        /// <returns>
        ///     The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitExtension(Expression node)
        {
            if (node is NullConditionalExpression)
            {
                return node;
            }

            return base.VisitExtension(node);
        }
    }
}
