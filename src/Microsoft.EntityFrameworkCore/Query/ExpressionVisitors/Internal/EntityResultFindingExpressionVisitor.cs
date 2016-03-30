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
    public class EntityResultFindingExpressionVisitor : ExpressionVisitorBase
    {
        private readonly IModel _model;
        private readonly IEntityTrackingInfoFactory _entityTrackingInfoFactory;
        private readonly QueryCompilationContext _queryCompilationContext;

        private List<EntityTrackingInfo> _entityTrackingInfos;

        public EntityResultFindingExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] IEntityTrackingInfoFactory entityTrackingInfoFactory,
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            _model = model;
            _entityTrackingInfoFactory = entityTrackingInfoFactory;
            _queryCompilationContext = queryCompilationContext;
        }

        public virtual IReadOnlyCollection<EntityTrackingInfo> FindEntitiesInResult([NotNull] Expression expression)
        {
            _entityTrackingInfos = new List<EntityTrackingInfo>();

            Visit(expression);

            return _entityTrackingInfos;
        }

        protected override Expression VisitQuerySourceReference(
            QuerySourceReferenceExpression expression)
        {
            var maybeEntityType
                = expression.Type.TryGetSequenceType() ?? expression.Type;

            var entityType
                = _model.FindEntityType(maybeEntityType)
                  ?? _model.FindEntityType(expression.Type);

            if (entityType != null)
            {
                _entityTrackingInfos.Add(
                    _entityTrackingInfoFactory
                        .Create(_queryCompilationContext, expression, entityType));
            }

            return expression;
        }

        // Prune these nodes...

        protected override Expression VisitSubQuery(SubQueryExpression expression) => expression;

        protected override Expression VisitMember(MemberExpression node) => node;

        protected override Expression VisitMethodCall(MethodCallExpression node) => node;

        protected override Expression VisitConditional(ConditionalExpression node) => node;

        protected override Expression VisitBinary(BinaryExpression node) => node;

        protected override Expression VisitTypeBinary(TypeBinaryExpression node) => node;

        protected override Expression VisitLambda<T>(Expression<T> node) => node;

        protected override Expression VisitInvocation(InvocationExpression node) => node;
    }
}
