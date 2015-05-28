// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class EntityResultFindingExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly ISet<IQuerySource> _untrackedQuerySources;

        private List<EntityTrackingInfo> _entityTrackingInfos;

        public EntityResultFindingExpressionTreeVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _queryCompilationContext = queryCompilationContext;

            _untrackedQuerySources
                = new HashSet<IQuerySource>(
                    _queryCompilationContext
                        .GetCustomQueryAnnotations(EntityFrameworkQueryableExtensions.AsNoTrackingMethodInfo)
                        .Select(qa => qa.QuerySource));
        }

        public virtual IReadOnlyCollection<EntityTrackingInfo> FindEntitiesInResult([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _entityTrackingInfos = new List<EntityTrackingInfo>();

            VisitExpression(expression);

            return _entityTrackingInfos;
        }

        protected override Expression VisitQuerySourceReferenceExpression(
            QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            if (!_untrackedQuerySources.Contains(querySourceReferenceExpression.ReferencedQuerySource))
            {
                var entityType
                    = _queryCompilationContext.Model
                        .FindEntityType(querySourceReferenceExpression.Type);

                if (entityType != null)
                {
                    var entityTrackingInfo
                        = new EntityTrackingInfo(
                            _queryCompilationContext, querySourceReferenceExpression, entityType);

                    _entityTrackingInfos.Add(entityTrackingInfo);
                }
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
