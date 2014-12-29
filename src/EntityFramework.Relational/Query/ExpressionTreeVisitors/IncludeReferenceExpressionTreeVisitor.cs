// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class IncludeReferenceExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private readonly IQuerySource _querySource;
        private readonly IReadOnlyList<INavigation> _navigationPath;
        private readonly IReadOnlyList<int> _readerOffsets;

        public IncludeReferenceExpressionTreeVisitor(
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<int> readerOffsets)
        {
            Check.NotNull(querySource, "querySource");
            Check.NotNull(navigationPath, "navigationPath");

            _querySource = querySource;
            _navigationPath = navigationPath;
            _readerOffsets = readerOffsets;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo))
            {
                var querySource = ((ConstantExpression)expression.Arguments[0]).Value;

                if (querySource == _querySource)
                {
                    return Expression.Call(
                        _includeReferenceMethodInfo
                            .MakeGenericMethod(expression.Method.GetGenericArguments()[0]),
                        expression.Arguments[1],
                        expression,
                        Expression.Constant(_navigationPath),
                        Expression.NewArrayInit(
                            typeof(Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>),
                            _readerOffsets.Select(
                                i => Expression.Lambda(
                                    Expression.Call(
                                        _getRelatedValueReadersMethodInfo,
                                        Expression.Convert(expression.Arguments[1], typeof(RelationalQueryContext)),
                                        expression.Arguments[3],
                                        Expression.Constant(i)),
                                    Expression.Parameter(typeof(EntityKey)),
                                    Expression.Parameter(typeof(Func<IValueReader, EntityKey>))))));
                }
            }

            return base.VisitMethodCallExpression(expression);
        }

        private static readonly MethodInfo _includeReferenceMethodInfo
            = typeof(IncludeReferenceExpressionTreeVisitor).GetTypeInfo()
                .GetDeclaredMethod("_IncludeReference");

        [UsedImplicitly]
        private static QuerySourceScope<TEntity> _IncludeReference<TEntity>(
            QueryContext queryContext,
            QuerySourceScope<TEntity> querySourceScope,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>> relatedValueReaders)
        {
            queryContext.QueryBuffer
                .Include(
                    querySourceScope._result,
                    navigationPath,
                    relatedValueReaders);

            return querySourceScope;
        }

        private static readonly MethodInfo _getRelatedValueReadersMethodInfo
            = typeof(IncludeReferenceExpressionTreeVisitor).GetTypeInfo()
                .GetDeclaredMethod("_GetRelatedValueReaders");

        [UsedImplicitly]
        private static IEnumerable<IValueReader> _GetRelatedValueReaders(
            RelationalQueryContext relationalQueryContext, DbDataReader dataReader, int readerOffset)
        {
            return new[]
                {
                    new OffsetValueReaderDecorator(
                        relationalQueryContext.ValueReaderFactory.Create(dataReader),
                        readerOffset)
                };
        }
    }
}
