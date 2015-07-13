// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class EntityResultFindingExpressionVisitor : ExpressionVisitorBase
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly ISet<IQuerySource> _untrackedQuerySources;

        private List<EntityTrackingInfo> _entityTrackingInfos;

        public EntityResultFindingExpressionVisitor(
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

            Visit(expression);

            return _entityTrackingInfos;
        }

        protected override Expression VisitQuerySourceReference(
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

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            return expression;
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            return expression;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression expression)
        {
            return expression;
        }

        protected override Expression VisitLambda<T>(Expression<T> expression)
        {
            return expression;
        }

        protected override Expression VisitInvocation(InvocationExpression expression)
        {
            return expression;
        }
    }
}
