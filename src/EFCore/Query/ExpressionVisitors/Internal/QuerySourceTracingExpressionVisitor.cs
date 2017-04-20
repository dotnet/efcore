// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QuerySourceTracingExpressionVisitor : ExpressionVisitorBase
    {
        private IQuerySource _targetQuerySource;
        private QuerySourceReferenceExpression _originQuerySourceReferenceExpression;

        private bool _reachable;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            return node.NodeType == ExpressionType.Coalesce
                ? base.VisitBinary(node)
                : node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.IfTrue);
            Visit(node.IfFalse);

            return node;
        }

        // Prune these nodes...

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
            => node.Expression.RemoveConvert() is QuerySourceReferenceExpression
                ? node
                : base.VisitMember(node);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitTypeBinary(TypeBinaryExpression node) => node;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> node) => node;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitInvocation(InvocationExpression node) => node;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
            => EntityQueryModelVisitor.IsPropertyMethod(node.Method)
                ? node
                : base.VisitMethodCall(node);
    }
}
