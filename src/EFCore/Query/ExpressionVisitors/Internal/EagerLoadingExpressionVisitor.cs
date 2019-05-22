// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EagerLoadingExpressionVisitor : QueryModelVisitorBase
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly QuerySourceTracingExpressionVisitor _querySourceTracingExpressionVisitor;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EagerLoadingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
        {
            _queryCompilationContext = queryCompilationContext;

            _querySourceTracingExpressionVisitor = querySourceTracingExpressionVisitorFactory.Create();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            ApplyIncludesForEagerLoadedNavigations(new QuerySourceReferenceExpression(fromClause), queryModel);

            base.VisitMainFromClause(fromClause, queryModel);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
        {
            foreach (var querySource in bodyClauses.OfType<IQuerySource>())
            {
                ApplyIncludesForEagerLoadedNavigations(new QuerySourceReferenceExpression(querySource), queryModel);
            }

            base.VisitBodyClauses(bodyClauses, queryModel);
        }

        private void ApplyIncludesForEagerLoadedNavigations(QuerySourceReferenceExpression querySourceReferenceExpression, QueryModel queryModel)
        {
            if (_querySourceTracingExpressionVisitor
                    .FindResultQuerySourceReferenceExpression(
                        queryModel.SelectClause.Selector,
                        querySourceReferenceExpression.ReferencedQuerySource) != null)
            {
                var entityType = _queryCompilationContext.Model.FindEntityType(querySourceReferenceExpression.Type);

                if (entityType != null)
                {
                    var stack = new Stack<INavigation>();

                    WalkNavigations(querySourceReferenceExpression, entityType, stack);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void WalkNavigations(Expression querySourceReferenceExpression, IEntityType entityType, Stack<INavigation> stack)
        {
            var outboundNavigations
                = entityType.GetNavigations()
                    .Concat(entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations()))
                    .Where(ShouldInclude)
                    .ToList();

            if (outboundNavigations.Count == 0
                && stack.Count > 0)
            {
                _queryCompilationContext.AddAnnotations(
                    new[]
                    {
                        new IncludeResultOperator(
                            stack.Reverse().ToArray(),
                            querySourceReferenceExpression,
                            implicitLoad: true)
                    });
            }
            else
            {
                foreach (var navigation in outboundNavigations)
                {
                    stack.Push(navigation);

                    WalkNavigations(querySourceReferenceExpression, navigation.GetTargetType(), stack);

                    stack.Pop();
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ShouldInclude(INavigation navigation)
            => navigation.IsEagerLoaded();
    }
}
