// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class EntityResultFindingExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        private readonly IModel _model;

        private List<EntityTrackingInfo> _entityTrackingInfos;

        public EntityResultFindingExpressionTreeVisitor([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        public virtual IEnumerable<EntityTrackingInfo> FindEntitiesInResult([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _entityTrackingInfos = new List<EntityTrackingInfo>();

            VisitExpression(expression);

            return _entityTrackingInfos;
        }

        protected override Expression VisitQuerySourceReferenceExpression(
            QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            var entityType = _model.FindEntityType(querySourceReferenceExpression.Type);

            if (entityType != null)
            {
                _entityTrackingInfos.Add(new EntityTrackingInfo(querySourceReferenceExpression, entityType));
            }

            return querySourceReferenceExpression;
        }

        // Prune these nodes...

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            return expression;
        }

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
