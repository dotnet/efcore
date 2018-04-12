// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A base class for SQL JOIN expressions.
    /// </summary>
    public abstract class JoinExpressionBase : TableExpressionBase
    {
        private TableExpressionBase _tableExpression;

        /// <summary>
        ///     Specialized constructor for use only by derived class.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        protected JoinExpressionBase([NotNull] TableExpressionBase tableExpression)
            : base(
                Check.NotNull(tableExpression, nameof(tableExpression)).QuerySource,
                tableExpression.Alias)
        {
            _tableExpression = tableExpression;
        }

        /// <summary>
        ///     The target table expression.
        /// </summary>
        public virtual TableExpressionBase TableExpression => _tableExpression;

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(Expression)" /> method passing the
        ///     reduced expression.
        ///     Throws an exception if the node isn't reducible.
        /// </summary>
        /// <param name="visitor"> An instance of <see cref="ExpressionVisitor" />. </param>
        /// <returns> The expression being visited, or an expression which should replace it in the tree. </returns>
        /// <remarks>
        ///     Override this method to provide logic to walk the node's children.
        ///     A typical implementation will call visitor.Visit on each of its
        ///     children, and if any of them change, should return a new copy of
        ///     itself with the modified children.
        /// </remarks>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            _tableExpression = (TableExpressionBase)visitor.Visit(_tableExpression);

            return this;
        }
    }
}
