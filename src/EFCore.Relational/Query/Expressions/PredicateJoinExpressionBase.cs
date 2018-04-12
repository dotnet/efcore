// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A base class for SQL JOIN expressions that have predicates.
    /// </summary>
    public abstract class PredicateJoinExpressionBase : JoinExpressionBase
    {
        private Expression _predicate;

        /// <summary>
        ///     Specialized constructor for use only by derived class.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        protected PredicateJoinExpressionBase([NotNull] TableExpressionBase tableExpression)
            : base(Check.NotNull(tableExpression, nameof(tableExpression)))
        {
        }

        /// <summary>
        ///     Gets or sets the predicate.
        /// </summary>
        /// <value>
        ///     The predicate.
        /// </value>
        public virtual Expression Predicate
        {
            get => _predicate;
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _predicate = value;
            }
        }

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
            base.VisitChildren(visitor);

            _predicate = visitor.Visit(_predicate);

            return this;
        }
    }
}
