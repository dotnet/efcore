// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryModelVisitor : EntityQueryModelVisitor
    {
        public InMemoryQueryModelVisitor([NotNull] QueryCompilationContext queryCompilationContext)
            : base(Check.NotNull(queryCompilationContext, "queryCompilationContext"))
        {
        }

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            return new InMemoryEntityQueryableExpressionTreeVisitor(this, querySource);
        }

        protected override void IncludeNavigation(
            IQuerySource querySource,
            Type resultType,
            LambdaExpression accessorLambda,
            INavigation navigation)
        {
            Check.NotNull(querySource, "querySource");
            Check.NotNull(resultType, "resultType");
            Check.NotNull(accessorLambda, "accessorLambda");
            Check.NotNull(navigation, "navigation");

            var inMemoryQueryCompilationContext
                = ((InMemoryQueryCompilationContext)QueryCompilationContext);

            var targetTable
                = inMemoryQueryCompilationContext.Database
                    .GetTable(navigation.GetTargetType());

            Expression
                = Expression.Call(
                    _includeMethodInfo.MakeGenericMethod(resultType),
                    QueryContextParameter,
                    Expression,
                    Expression.Constant(navigation),
                    Expression.Constant(targetTable),
                    accessorLambda);
        }

        private static readonly MethodInfo _includeMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("Include");

        [UsedImplicitly]
        private static IEnumerable<TResult> Include<TResult>(
            QueryContext queryContext,
            IEnumerable<TResult> source,
            INavigation navigation,
            InMemoryDatabase.InMemoryTable targetTable,
            Func<TResult, object> accessorLambda)
        {
            var inMemoryQueryContext = ((InMemoryQueryContext)queryContext);

            return
                source
                    .Select(result =>
                        {
                            inMemoryQueryContext.QueryBuffer
                                .Include(
                                    accessorLambda.Invoke(result),
                                    navigation,
                                    (primaryKey, relatedKeyFactory) =>
                                        targetTable
                                            .Select(vs => new ObjectArrayValueReader(vs))
                                            .Where(valueReader => relatedKeyFactory(valueReader).Equals(primaryKey))
                                );

                            return result;
                        });
        }

        private static readonly MethodInfo _entityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("EntityQuery");

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityQuery<TEntity>(
            QueryContext queryContext,
            IEntityType entityType,
            InMemoryDatabase.InMemoryTable inMemoryTable, 
            bool queryStateManager)
        {
            return inMemoryTable
                .Select(t => (TEntity)queryContext.QueryBuffer
                    .GetEntity(entityType, new ObjectArrayValueReader(t), queryStateManager));
        }

        private static readonly MethodInfo _projectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("ProjectionQuery");

        [UsedImplicitly]
        private static IEnumerable<IValueReader> ProjectionQuery(InMemoryDatabase.InMemoryTable inMemoryTable)
        {
            return inMemoryTable.Select(t => new ObjectArrayValueReader(t));
        }

        private class InMemoryEntityQueryableExpressionTreeVisitor : EntityQueryableExpressionTreeVisitor
        {
            private readonly IQuerySource _querySource;

            public InMemoryEntityQueryableExpressionTreeVisitor(
                InMemoryQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
                : base(entityQueryModelVisitor)
            {
                _querySource = querySource;
            }

            private new InMemoryQueryModelVisitor QueryModelVisitor
            {
                get { return (InMemoryQueryModelVisitor)base.QueryModelVisitor; }
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                Check.NotNull(elementType, "elementType");

                var entityType
                    = QueryModelVisitor.QueryCompilationContext.Model
                        .GetEntityType(elementType);

                var inMemoryTable
                    = ((InMemoryQueryCompilationContext)QueryModelVisitor.QueryCompilationContext).Database
                        .GetTable(entityType);

                if (QueryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    return Expression.Call(
                        _entityQueryMethodInfo.MakeGenericMethod(elementType),
                        QueryContextParameter,
                        Expression.Constant(entityType),
                        Expression.Constant(inMemoryTable), 
                        Expression.Constant(QueryModelVisitor.QuerySourceRequiresTracking(_querySource)));
                }

                return Expression.Call(_projectionQueryMethodInfo, Expression.Constant(inMemoryTable));
            }
        }
    }
}
