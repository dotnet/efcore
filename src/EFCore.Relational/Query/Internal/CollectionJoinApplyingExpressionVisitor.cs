// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CollectionJoinApplyingExpressionVisitor : ExpressionVisitor
    {
        private readonly bool _splitQuery;
        private readonly bool _noConfiguredBehavior;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private int _collectionId;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CollectionJoinApplyingExpressionVisitor([NotNull] RelationalQueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _splitQuery = queryCompilationContext.QuerySplittingBehavior == QuerySplittingBehavior.SplitQuery;
            _noConfiguredBehavior = queryCompilationContext.QuerySplittingBehavior == null;
            _logger = queryCompilationContext.Logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            if (extensionExpression is CollectionShaperExpression collectionShaperExpression)
            {
                var collectionId = _collectionId++;

                if (_noConfiguredBehavior
                    && _collectionId == 2)
                {
                    _logger.MultipleCollectionIncludeWarning();
                }

                var projectionBindingExpression = (ProjectionBindingExpression)collectionShaperExpression.Projection;
                var selectExpression = (SelectExpression)projectionBindingExpression.QueryExpression;
                // Do pushdown beforehand so it updates all pending collections first
                if (selectExpression.IsDistinct
                    || selectExpression.Limit != null
                    || selectExpression.Offset != null
                    || selectExpression.GroupBy.Count > 0)
                {
                    selectExpression.PushdownIntoSubquery();
                }

                var innerShaper = Visit(collectionShaperExpression.InnerShaper);

                var collectionJoin = selectExpression.ApplyCollectionJoin(
                    projectionBindingExpression.Index.Value,
                    collectionId,
                    innerShaper,
                    collectionShaperExpression.Navigation,
                    collectionShaperExpression.ElementType,
                    _splitQuery);

                if (_splitQuery
                    && collectionJoin == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.UnableToSplitCollectionProjectionInSplitQuery(
                            $"{nameof(QuerySplittingBehavior)}.{QuerySplittingBehavior.SplitQuery}",
                            nameof(RelationalQueryableExtensions.AsSplitQuery),
                            nameof(RelationalQueryableExtensions.AsSingleQuery)));
                }

                return collectionJoin;
            }

            return extensionExpression is ShapedQueryExpression shapedQueryExpression
                ? shapedQueryExpression.UpdateShaperExpression(Visit(shapedQueryExpression.ShaperExpression))
                : base.VisitExtension(extensionExpression);
        }
    }
}
