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

            var entityTypeParameter = Expression.Parameter(typeof(IEntityType));
            var primaryKeyParameter = Expression.Parameter(typeof(EntityKey));
            var relatedKeyFactoryParameter = Expression.Parameter(typeof(Func<IValueReader, EntityKey>));

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
                                            _getRelatedValueReadersMethodInfo,
                                            QueryContextParameter,
                                            Expression.Constant(targetType),
                                            primaryKeyParameter,
                                            relatedKeyFactoryParameter,
                                            materializer),
                                        entityTypeParameter,
                                        primaryKeyParameter,
                                        relatedKeyFactoryParameter);
                                })),
                    Expression.Constant(querySourceRequiresTracking));
        }

        private static readonly MethodInfo _includeMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("Include");

        [UsedImplicitly]
        private static IEnumerable<TResult> Include<TResult>(
            QueryContext queryContext,
            IEnumerable<TResult> source,
            IReadOnlyList<INavigation> navigationPath,
            Func<TResult, object> accessorLambda,
            IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            return
                source
                    .Select(result =>
                        {
                            queryContext.QueryBuffer
                                .Include(
                                    accessorLambda.Invoke(result),
                                    navigationPath,
                                    relatedEntitiesLoaders,
                                    querySourceRequiresTracking);

                            return result;
                        });
        }

        private static readonly MethodInfo _getRelatedValueReadersMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("GetRelatedValueReaders");

        [UsedImplicitly]
        private static IEnumerable<EntityLoadInfo> GetRelatedValueReaders(
            QueryContext queryContext,
            IEntityType targetType,
            EntityKey primaryKey,
            Func<IValueReader, EntityKey> relatedKeyFactory,
            Func<IEntityType, IValueReader, object> materializer)
        {
            return ((InMemoryQueryContext)queryContext).Database
                .GetTables(targetType)
                .SelectMany(t =>
                    t.Select(vs => new EntityLoadInfo(
                        new ObjectArrayValueReader(vs), vr => materializer(t.EntityType, vr)))
                        .Where(eli => relatedKeyFactory(eli.ValueReader).Equals(primaryKey)));
        }

        private static readonly MethodInfo _entityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("EntityQuery");

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityQuery<TEntity>(
            QueryContext queryContext,
            IEntityType entityType,
            Func<IValueReader, EntityKey> entityKeyFactory,
            Func<IEntityType, IValueReader, object> materializer,
            bool queryStateManager)
            where TEntity : class
        {
            return ((InMemoryQueryContext)queryContext).Database
                .GetTables(entityType)
                .SelectMany(t =>
                    t.Select(vs =>
                        {
                            var valueReader = new ObjectArrayValueReader(vs);
                            var entityKey = entityKeyFactory(valueReader);

                            return (TEntity)queryContext
                                .QueryBuffer
                                .GetEntity(
                                    entityType,
                                    entityKey,
                                    new EntityLoadInfo(
                                        valueReader,
                                        vr => materializer(t.EntityType, vr)),
                                    queryStateManager);
                        }));
        }

        private static readonly MethodInfo _projectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("ProjectionQuery");

        [UsedImplicitly]
        private static IEnumerable<IValueReader> ProjectionQuery(
            QueryContext queryContext,
            IEntityType entityType)
        {
            return ((InMemoryQueryContext)queryContext).Database
                .GetTables(entityType)
                .SelectMany(t => t.Select(vs => new ObjectArrayValueReader(vs)));
        }

        private class InMemoryEntityQueryableExpressionTreeVisitor : EntityQueryableExpressionTreeVisitor
        {
            private readonly IQuerySource _querySource;

            public InMemoryEntityQueryableExpressionTreeVisitor(
                EntityQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
                : base(entityQueryModelVisitor)
            {
                _querySource = querySource;
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

                Func<IValueReader, EntityKey> entityKeyFactory
                    = vr => keyFactory.Create(entityType, keyProperties, vr);

                var materializer
                    = new MaterializerFactory(QueryModelVisitor
                        .QueryCompilationContext
                        .EntityMaterializerSource)
                        .CreateMaterializer(entityType);

                if (QueryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    return Expression.Call(
                        _entityQueryMethodInfo.MakeGenericMethod(elementType),
                        QueryContextParameter,
                        Expression.Constant(entityType),
                        Expression.Constant(entityKeyFactory),
                        materializer,
                        Expression.Constant(QueryModelVisitor.QuerySourceRequiresTracking(_querySource)));
                }

                return Expression.Call(
                    _projectionQueryMethodInfo,
                    QueryContextParameter,
                    Expression.Constant(entityType));
            }
        }
    }
}
