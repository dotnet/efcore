// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class QuerySourceTracingExpressionTreeVisitor : ExpressionTreeVisitorBase
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

            VisitExpression(expression);

            return _reachable ? _originQuerySourceReferenceExpression : null;
        }

        protected override Expression VisitQuerySourceReferenceExpression(QuerySourceReferenceExpression expression)
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
                        VisitExpression(fromClauseBase.FromExpression);
                    }
                }

                if (!_reachable)
                {
                    _originQuerySourceReferenceExpression = null;
                }
            }

            return expression;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            VisitExpression(expression.QueryModel.SelectClause.Selector);

            return expression;
        }

        // Prune these nodes...

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            return expression;
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            return expression;
        }

        protected override Expression VisitConditionalExpression(ConditionalExpression expression)
        {
            return expression;
        }

        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            return expression;
        }

        protected override Expression VisitTypeBinaryExpression(TypeBinaryExpression expression)
        {
            return expression;
        }

        protected override Expression VisitLambdaExpression(LambdaExpression expression)
        {
            return expression;
        }

        protected override Expression VisitInvocationExpression(InvocationExpression expression)
        {
            return expression;
        }
    }
}
