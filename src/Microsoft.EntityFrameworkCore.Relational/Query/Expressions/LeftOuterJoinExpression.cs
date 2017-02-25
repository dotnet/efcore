// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL LEFT OUTER JOIN expression.
    /// </summary>
    public class LeftOuterJoinExpression : PredicateJoinExpressionBase
    {
        /// <summary>
        ///     Creates a new instance of LeftOuterJoinExpression.
        /// </summary>
        /// <param name="tableExpression"></param>
        public LeftOuterJoinExpression([NotNull] TableExpressionBase tableExpression)
            : base(Check.NotNull(tableExpression, nameof(tableExpression)))
        {
        }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitLeftOuterJoin(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Determines whether or not this LeftOuterJoinExpression handles the given query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     true if the supplied query source is handled by this LeftOuterJoinExpression; otherwise false.
        /// </returns>
        public override bool HandlesQuerySource(IQuerySource querySource)
        {
            if (querySource is AdditionalFromClause additionalFromClause)
            {
                var subQueryModel = (additionalFromClause.FromExpression as SubQueryExpression)?.QueryModel;
                if (subQueryModel != null
                    && !subQueryModel.BodyClauses.Any()
                    && subQueryModel.ResultOperators.Count == 1
                    && subQueryModel.ResultOperators.Single() is DefaultIfEmptyResultOperator
                    && (subQueryModel.SelectClause.Selector as QuerySourceReferenceExpression)?.ReferencedQuerySource == subQueryModel.MainFromClause
                    && (subQueryModel.MainFromClause.FromExpression as QuerySourceReferenceExpression)?.ReferencedQuerySource is GroupJoinClause targetGroupJoin)
                {
                    if (QuerySource == targetGroupJoin.JoinClause)
                    {
                        return true;
                    }
                }
            }

            return base.HandlesQuerySource(querySource);
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString()
            => "LEFT OUTER JOIN (" + TableExpression + ") ON " + Predicate;
    }
}