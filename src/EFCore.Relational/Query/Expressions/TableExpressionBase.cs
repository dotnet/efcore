// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A base class for SQL table expressions.
    /// </summary>
    public abstract class TableExpressionBase : Expression
    {
        private string _alias;
        private IQuerySource _querySource;

        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.TableExpressionBase class.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="alias"> The alias. </param>
        protected TableExpressionBase(
            [CanBeNull] IQuerySource querySource, [CanBeNull] string alias)
        {
            _querySource = querySource;
            _alias = alias;
        }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => typeof(object);

        /// <summary>
        ///     Gets the query source.
        /// </summary>
        /// <value>
        ///     The query source.
        /// </value>
        public virtual IQuerySource QuerySource
        {
            get => _querySource;
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _querySource = value;
            }
        }

        /// <summary>
        ///     Gets the alias.
        /// </summary>
        /// <value>
        ///     The alias.
        /// </value>
        public virtual string Alias
        {
            get => _alias;
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _alias = value;
            }
        }

        /// <summary>
        ///     Determines whether or not this TableExpressionBase handles the given query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     true if the supplied query source is handled by this TableExpressionBase; otherwise false.
        /// </returns>
        public virtual bool HandlesQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _querySource == PreProcessQuerySource(querySource);
        }

        /// <summary>
        ///     Pre-processes the given <see cref="IQuerySource" />.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns> The query source after pre-processing. </returns>
        protected virtual IQuerySource PreProcessQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            // TODO: DRY this up with include pipeline trying to find innerQSRE in similar cases
            var innerQsre = (querySource as FromClauseBase)?.FromExpression as QuerySourceReferenceExpression;
            if (innerQsre?.Type.IsGrouping() == true)
            {
                var groupByResultOperator =
                    (GroupResultOperator)((SubQueryExpression)((MainFromClause)innerQsre.ReferencedQuerySource).FromExpression)
                    .QueryModel.ResultOperators
                    .Last();

                var innerQuerySource = groupByResultOperator.ElementSelector.TryGetReferencedQuerySource();

                if (innerQuerySource != null)
                {
                    return innerQuerySource;
                }
            }

            var newQuerySource = (querySource as AdditionalFromClause)?.TryGetFlattenedGroupJoinClause() ?? querySource;

            return (newQuerySource as GroupJoinClause)?.JoinClause ?? newQuerySource;
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
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
