// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class QuerySourceTracingExpressionVisitor : ExpressionVisitorBase
    {
        private IQuerySource _targetQuerySource;
        private QuerySourceReferenceExpression _originQuerySourceReferenceExpression;
        private QueryModel _originGroupByQueryModel;

        private bool _reachable;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual QueryModel OriginGroupByQueryModel => _originGroupByQueryModel;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                    if (expression.ReferencedQuerySource is FromClauseBase fromClauseBase)
                    {
                        Visit(fromClauseBase.FromExpression);
                    }

                    if (expression.ReferencedQuerySource is JoinClause joinClause)
                    {
                        Visit(joinClause.InnerSequence);
                    }

                    if (expression.ReferencedQuerySource is GroupJoinClause groupJoinClause)
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

        private static readonly ISet<Type> _aggregateResultOperators = new HashSet<Type>
        {
            typeof(AverageResultOperator),
            typeof(CountResultOperator),
            typeof(LongCountResultOperator),
            typeof(MaxResultOperator),
            typeof(MinResultOperator),
            typeof(SumResultOperator)
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            if (_reachable)
            {
                return subQueryExpression;
            }

            if (subQueryExpression.QueryModel.ResultOperators.Any(ro => _aggregateResultOperators.Contains(ro.GetType())))
            {
                return subQueryExpression;
            }

            _originGroupByQueryModel
                = subQueryExpression.QueryModel.ResultOperators
                    .Any(ro => ro is GroupResultOperator)
                    ? subQueryExpression.QueryModel
                    : _originGroupByQueryModel;

            return base.VisitSubQuery(subQueryExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            return node.NodeType == ExpressionType.Coalesce
                ? base.VisitBinary(node)
                : node;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.IfTrue);
            Visit(node.IfFalse);

            return node;
        }

        // Prune these nodes...

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
            => node.Expression.RemoveConvert() is QuerySourceReferenceExpression
                ? node
                : base.VisitMember(node);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitTypeBinary(TypeBinaryExpression node) => node;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> node) => node;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitInvocation(InvocationExpression node) => node;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
            => node.Method.IsEFPropertyMethod()
                ? node
                : base.VisitMethodCall(node);
    }
}
