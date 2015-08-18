// Copyright (c) .NET Foundation. All rights reserved.
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
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryModelVisitor : EntityQueryModelVisitor
    {
        public InMemoryQueryModelVisitor([NotNull] QueryCompilationContext queryCompilationContext)
            : base(Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)))
        {
        }

        protected override ExpressionVisitor CreateQueryingExpressionVisitor(IQuerySource querySource)
            => new InMemoryEntityQueryableExpressionVisitor(this, querySource);

        protected override void IncludeNavigations(
            IncludeSpecification includeSpecification,
            Type resultType,
            LambdaExpression accessorLambda,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));
            Check.NotNull(accessorLambda, nameof(accessorLambda));

            var primaryKeyParameter = Expression.Parameter(typeof(EntityKey));
            var relatedKeyFactoryParameter = Expression.Parameter(typeof(Func<ValueBuffer, EntityKey>));
            var navigationPath = includeSpecification.NavigationPath;

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
            return ((InMemoryQueryContext)queryContext).Store
                .GetTables(targetType)
                .SelectMany(t =>
                    t.Select(vs => new EntityLoadInfo(
                        new ValueBuffer(vs), vb => materializer(t.EntityType, vb)))
                        .Where(eli => relatedKeyFactory(eli.ValueBuffer).Equals(primaryKey)));
        }

        private static readonly MethodInfo _entityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(EntityQuery));

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityQuery<TEntity>(
            QueryContext queryContext,
            IEntityType entityType,
            Func<ValueBuffer, EntityKey> entityKeyFactory,
            Func<IEntityType, ValueBuffer, object> materializer,
            bool queryStateManager)
            where TEntity : class
        {
            return ((InMemoryQueryContext)queryContext).Store
                .GetTables(entityType)
                .SelectMany(t =>
                    t.Select(vs =>
                        {
                            var valueBuffer = new ValueBuffer(vs);
                            var entityKey = entityKeyFactory(valueBuffer);

                            return (TEntity)queryContext
                                .QueryBuffer
                                .GetEntity(
                                    entityType,
                                    entityKey,
                                    new EntityLoadInfo(
                                        valueBuffer,
                                        vr => materializer(t.EntityType, vr)),
                                    queryStateManager);
                        }));
        }

        private static readonly MethodInfo _projectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(ProjectionQuery));

        [UsedImplicitly]
        private static IEnumerable<ValueBuffer> ProjectionQuery(
            QueryContext queryContext,
            IEntityType entityType)
        {
            return ((InMemoryQueryContext)queryContext).Store
                .GetTables(entityType)
                .SelectMany(t => t.Select(vs => new ValueBuffer(vs)));
        }

        private class InMemoryEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
        {
            private readonly IQuerySource _querySource;

            public InMemoryEntityQueryableExpressionVisitor(
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
                        .EntityKeyFactorySource.GetKeyFactory(entityType.GetPrimaryKey());

                Func<ValueBuffer, EntityKey> entityKeyFactory
                    = vr => keyFactory.Create(keyProperties, vr);

                if (QueryModelVisitor.QueryCompilationContext
                    .QuerySourceRequiresMaterialization(_querySource))
                {
                    var materializer
                        = new MaterializerFactory(QueryModelVisitor
                            .QueryCompilationContext
                            .EntityMaterializerSource)
                            .CreateMaterializer(entityType);

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
