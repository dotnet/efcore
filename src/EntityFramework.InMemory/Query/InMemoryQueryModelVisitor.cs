// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryModelVisitor : EntityQueryModelVisitor
    {
        public InMemoryQueryModelVisitor([NotNull] QueryCompilationContext queryCompilationContext)
            : base(Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)))
        {
        }

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            return new InMemoryEntityQueryableExpressionTreeVisitor(this, querySource);
        }

        protected override void IncludeNavigations(
            IQuerySource querySource,
            Type resultType,
            LambdaExpression accessorLambda,
            IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(resultType, nameof(resultType));
            Check.NotNull(accessorLambda, nameof(accessorLambda));
            Check.NotNull(navigationPath, nameof(navigationPath));

            var primaryKeyParameter = Expression.Parameter(typeof(EntityKey));
            var relatedKeyFactoryParameter = Expression.Parameter(typeof(Func<ValueBuffer, EntityKey>));

            Expression
                = Expression.Call(
                    _includeMethodInfo.MakeGenericMethod(resultType),
                    QueryContextParameter,
                    Expression,
                    Expression.Constant(navigationPath),
                    accessorLambda,
                    Expression.NewArrayInit(
                        typeof(RelatedEntitiesLoader),
                        navigationPath.Select(
                            n =>
                                {
                                    var targetType = n.GetTargetType();

                                    var materializer
                                        = new MaterializerFactory(
                                            QueryCompilationContext
                                                .EntityMaterializerSource)
                                            .CreateMaterializer(targetType);

                                    return Expression.Lambda<RelatedEntitiesLoader>(
                                        Expression.Call(
                                            _getRelatedValueBuffersMethodInfo,
                                            QueryContextParameter,
                                            Expression.Constant(targetType),
                                            primaryKeyParameter,
                                            relatedKeyFactoryParameter,
                                            materializer),
                                        primaryKeyParameter,
                                        relatedKeyFactoryParameter);
                                })),
                    Expression.Constant(querySourceRequiresTracking));
        }

        private static readonly MethodInfo _includeMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Include));

        [UsedImplicitly]
        private static IEnumerable<QuerySourceScope<TResult>> Include<TResult>(
            QueryContext queryContext,
            IEnumerable<QuerySourceScope<TResult>> source,
            IReadOnlyList<INavigation> navigationPath,
            Func<TResult, object> accessorLambda,
            IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            return
                source
                    .Select(qss =>
                        {
                            if (qss != null
                                && qss.Result != null)
                            {
                                queryContext.QueryBuffer
                                    .Include(
                                        accessorLambda.Invoke(qss.Result),
                                        navigationPath,
                                        relatedEntitiesLoaders,
                                        querySourceRequiresTracking);
                            }

                            return qss;
                        });
        }

        private static readonly MethodInfo _getRelatedValueBuffersMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetRelatedValueBuffers));

        [UsedImplicitly]
        private static IEnumerable<EntityLoadInfo> GetRelatedValueBuffers(
            QueryContext queryContext,
            IEntityType targetType,
            EntityKey primaryKey,
            Func<ValueBuffer, EntityKey> relatedKeyFactory,
            Func<IEntityType, ValueBuffer, object> materializer)
        {
            return ((InMemoryQueryContext)queryContext).Database
                .GetTables(targetType)
                .SelectMany(t =>
                    t.Select(vs =>
                        new EntityLoadInfo(
                            new ValueBuffer(vs), vr => materializer(t.EntityType, vr)))
                        .Where(eli => relatedKeyFactory(eli.ValueBuffer).Equals(primaryKey)));
        }

        private static readonly MethodInfo _entityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(EntityQuery));

        [UsedImplicitly]
        private static IEnumerable<QuerySourceScope<TEntity>> EntityQuery<TEntity>(
            IQuerySource querySource,
            QueryContext queryContext,
            QuerySourceScope parentQuerySourceScope,
            IEntityType entityType,
            Func<ValueBuffer, EntityKey> entityKeyFactory,
            Func<IEntityType, ValueBuffer, object> materializer,
            bool queryStateManager)
            where TEntity : class
        {
            return ((InMemoryQueryContext)queryContext).Database
                .GetTables(entityType)
                .SelectMany(t =>
                    t.Select(vs =>
                        {
                            var valueBuffer = new ValueBuffer(vs);

                            return
                                new QuerySourceScope<TEntity>(
                                    querySource,
                                    (TEntity)queryContext
                                        .QueryBuffer
                                        .GetEntity(
                                            entityType,
                                            entityKeyFactory(valueBuffer),
                                            new EntityLoadInfo(
                                                valueBuffer,
                                                vr => materializer(t.EntityType, vr)),
                                            queryStateManager),
                                    parentQuerySourceScope,
                                    valueBuffer);
                        }));
        }

        private static readonly MethodInfo _projectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(ProjectionQuery));

        [UsedImplicitly]
        private static IEnumerable<QuerySourceScope<ValueBuffer>> ProjectionQuery(
            IQuerySource querySource,
            QueryContext queryContext,
            QuerySourceScope parentQuerySourceScope,
            IEntityType entityType)
        {
            return ((InMemoryQueryContext)queryContext).Database
                .GetTables(entityType)
                .SelectMany(t =>
                    t.Select(vs =>
                        new QuerySourceScope<ValueBuffer>(
                            querySource,
                            new ValueBuffer(vs),
                            parentQuerySourceScope,
                            new ValueBuffer())));
        }

        private class InMemoryEntityQueryableExpressionTreeVisitor : EntityQueryableExpressionTreeVisitor
        {
            public InMemoryEntityQueryableExpressionTreeVisitor(
                EntityQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
                : base(entityQueryModelVisitor, querySource)
            {
            }

            private new InMemoryQueryModelVisitor QueryModelVisitor => (InMemoryQueryModelVisitor)base.QueryModelVisitor;

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                Check.NotNull(elementType, nameof(elementType));

                var entityType
                    = QueryModelVisitor.QueryCompilationContext.Model
                        .GetEntityType(elementType);

                var keyProperties
                    = entityType.GetPrimaryKey().Properties;

                var keyFactory
                    = QueryModelVisitor
                        .QueryCompilationContext
                        .EntityKeyFactorySource.GetKeyFactory(keyProperties);

                Func<ValueBuffer, EntityKey> entityKeyFactory
                    = vr => keyFactory.Create(entityType, keyProperties, vr);

                var materializer
                    = new MaterializerFactory(QueryModelVisitor
                        .QueryCompilationContext
                        .EntityMaterializerSource)
                        .CreateMaterializer(entityType);

                if (QueryModelVisitor.QuerySourceRequiresMaterialization(QuerySource))
                {
                    return Expression.Call(
                        _entityQueryMethodInfo.MakeGenericMethod(elementType),
                        Expression.Constant(QuerySource),
                        QueryContextParameter,
                        QuerySourceScopeParameter,
                        Expression.Constant(entityType),
                        Expression.Constant(entityKeyFactory),
                        materializer,
                        Expression.Constant(QueryModelVisitor.QuerySourceRequiresTracking(QuerySource)));
                }

                return Expression.Call(
                    _projectionQueryMethodInfo,
                    Expression.Constant(QuerySource),
                    QueryContextParameter,
                    QuerySourceScopeParameter,
                    Expression.Constant(entityType));
            }
        }
    }
}
