// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class QuerySourceTracingExpressionVisitor : ExpressionVisitorBase
    {
        private IQuerySource _targetQuerySource;
        private QuerySourceReferenceExpression _originQuerySourceReferenceExpression;

        private bool _reachable;

        public virtual QuerySourceReferenceExpression FindResultQuerySourceReferenceExpression(
            [NotNull] Expression expression,
            [NotNull] IQuerySource targetQuerySource)
        {
            _targetQuerySource = targetQuerySource;
            _originQuerySourceReferenceExpression = null;
            _reachable = false;

            Visit(expression);

            return _reachable ? _originQuerySourceReferenceExpression : null;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            if (!_reachable)
            {
                if (_originQuerySourceReferenceExpression == null)
                {
                    _originQuerySourceReferenceExpression = querySourceReferenceExpression;
                }

                if (querySourceReferenceExpression.ReferencedQuerySource.Equals(_targetQuerySource))
                {
                    _reachable = true;
                }
                else
                {
                    var fromClauseBase = querySourceReferenceExpression.ReferencedQuerySource as FromClauseBase;

                    if (fromClauseBase != null)
                    {
                        Visit(fromClauseBase.FromExpression);
                    }

                    var joinClause = querySourceReferenceExpression.ReferencedQuerySource as JoinClause;

                    if (joinClause != null)
                    {
                        Visit(joinClause.InnerSequence);
                    }

                    var groupJoinClause = querySourceReferenceExpression.ReferencedQuerySource as GroupJoinClause;

                    if (groupJoinClause != null)
                    {
                        Visit(groupJoinClause.JoinClause.InnerSequence);
                    }
                }

                if (!_reachable)
                {
                    _originQuerySourceReferenceExpression = null;
                }
            }

            return querySourceReferenceExpression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            Visit(subQueryExpression.QueryModel.SelectClause.Selector);

            return subQueryExpression;
        }

        // Prune these nodes...

        protected override Expression VisitMember(MemberExpression expression) => expression;

        protected override Expression VisitConditional(ConditionalExpression expression) => expression;

        protected override Expression VisitBinary(BinaryExpression expression) => expression;

        protected override Expression VisitTypeBinary(TypeBinaryExpression expression) => expression;

        protected override Expression VisitLambda<T>(Expression<T> expression) => expression;

        protected override Expression VisitInvocation(InvocationExpression expression) => expression;
    }
}
