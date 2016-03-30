// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
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

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            if (!_reachable)
            {
                if (_originQuerySourceReferenceExpression == null)
                {
                    _originQuerySourceReferenceExpression = expression;
                }

                if (expression.ReferencedQuerySource.Equals(_targetQuerySource))
                {
                    _reachable = true;
                }
                else
                {
                    var fromClauseBase = expression.ReferencedQuerySource as FromClauseBase;

                    if (fromClauseBase != null)
                    {
                        Visit(fromClauseBase.FromExpression);
                    }

                    var joinClause = expression.ReferencedQuerySource as JoinClause;

                    if (joinClause != null)
                    {
                        Visit(joinClause.InnerSequence);
                    }

                    var groupJoinClause = expression.ReferencedQuerySource as GroupJoinClause;

                    if (groupJoinClause != null)
                    {
                        if (groupJoinClause.JoinClause.Equals(_targetQuerySource))
                        {
                            _reachable = true;
                        }
                        else
                        {
                            Visit(groupJoinClause.JoinClause.InnerSequence);
                        }
                    }
                }

                if (!_reachable)
                {
                    _originQuerySourceReferenceExpression = null;
                }
            }

            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Visit(expression.QueryModel.SelectClause.Selector);

            return expression;
        }

        // Prune these nodes...

        protected override Expression VisitMember(MemberExpression node) => node;

        protected override Expression VisitConditional(ConditionalExpression node) => node;

        protected override Expression VisitBinary(BinaryExpression node) => node;

        protected override Expression VisitTypeBinary(TypeBinaryExpression node) => node;

        protected override Expression VisitLambda<T>(Expression<T> node) => node;

        protected override Expression VisitInvocation(InvocationExpression node) => node;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var referenceSource = node.Arguments.FirstOrDefault() as QuerySourceReferenceExpression;

            if (EntityQueryModelVisitor.IsPropertyMethod(node.Method)
                && referenceSource?.ReferencedQuerySource.Equals(_targetQuerySource) == true)
            {
                return node;
            }

            return base.VisitMethodCall(node);
        }
    }
}
