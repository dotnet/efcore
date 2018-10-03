// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityResultFindingExpressionVisitor : ExpressionVisitorBase
    {
        private readonly IModel _model;
        private readonly IEntityTrackingInfoFactory _entityTrackingInfoFactory;
        private readonly QueryCompilationContext _queryCompilationContext;

        private List<EntityTrackingInfo> _entityTrackingInfos;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityResultFindingExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] IEntityTrackingInfoFactory entityTrackingInfoFactory,
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            _model = model;
            _entityTrackingInfoFactory = entityTrackingInfoFactory;
            _queryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyCollection<EntityTrackingInfo> FindEntitiesInResult([NotNull] Expression expression)
        {
            _entityTrackingInfos = new List<EntityTrackingInfo>();

            Visit(expression);

            return _entityTrackingInfos;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitQuerySourceReference(
            QuerySourceReferenceExpression expression)
        {
            var sequenceType = expression.Type.TryGetSequenceType();

            var entityType
                = _queryCompilationContext.FindEntityType(expression.ReferencedQuerySource)
                  ?? ((sequenceType != null
                          ? _model.FindEntityType(sequenceType)
                          : null)
                      ?? _model.FindEntityType(expression.Type));

            if (entityType?.IsQueryType == false)
            {
                _entityTrackingInfos.Add(
                    _entityTrackingInfoFactory
                        .Create(_queryCompilationContext, expression, entityType));
            }

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitNew(NewExpression expression)
            => expression.Members == null ? expression : base.VisitNew(expression);

        // Prune these nodes...

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression) => expression;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node) => node;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node) => node;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node) => node;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node) => node;

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
    }
}
