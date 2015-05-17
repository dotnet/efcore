// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
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
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(targetQuerySource, nameof(targetQuerySource));

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

        protected override Expression VisitMember(MemberExpression expression) => expression;

        protected override Expression VisitMethodCall(MethodCallExpression expression) => expression;

        protected override Expression VisitConditional(ConditionalExpression expression) => expression;

        protected override Expression VisitBinary(BinaryExpression expression) => expression;

        protected override Expression VisitTypeBinary(TypeBinaryExpression expression) => expression;

        protected override Expression VisitLambda<T>(Expression<T> expression) => expression;

        protected override Expression VisitInvocation(InvocationExpression expression) => expression;
    }
}
