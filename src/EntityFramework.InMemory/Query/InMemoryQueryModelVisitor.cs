// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
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

        protected override void IncludeNavigations(
            IQuerySource querySource,
            Type resultType,
            LambdaExpression accessorLambda,
            IReadOnlyList<INavigation> navigationPath)
        {
            Check.NotNull(querySource, "querySource");
            Check.NotNull(resultType, "resultType");
            Check.NotNull(accessorLambda, "accessorLambda");
            Check.NotNull(navigationPath, "navigationPath");

            var inMemoryQueryCompilationContext
                = ((InMemoryQueryCompilationContext)QueryCompilationContext);

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
                        typeof(Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>),
                        navigationPath.Select(
                            n => Expression.Lambda(
                                Expression.Call(
                                    _getRelatedValueReadersMethodInfo,
                                    Expression.Constant(
                                        inMemoryQueryCompilationContext.Database.GetTable(n.GetTargetType())),
                                    primaryKeyParameter,
                                    relatedKeyFactoryParameter),
                                primaryKeyParameter,
                                relatedKeyFactoryParameter))));
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
            IReadOnlyList<Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>> relatedValueReaders)
        {
            return
                source
                    .Select(result =>
                        {
                            queryContext.QueryBuffer
                                .Include(
                                    accessorLambda.Invoke(result),
                                    navigationPath,
                                    relatedValueReaders);

                            return result;
                        });
        }

        private static readonly MethodInfo _getRelatedValueReadersMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("GetRelatedValueReaders");

        [UsedImplicitly]
        private static IEnumerable<IValueReader> GetRelatedValueReaders(
            InMemoryDatabase.InMemoryTable targetTable,
            EntityKey primaryKey,
            Func<IValueReader, EntityKey> relatedKeyFactory)
        {
            return targetTable
                .Select(vs => new ObjectArrayValueReader(vs))
                .Where(valueReader => relatedKeyFactory(valueReader).Equals(primaryKey));
        }

        private static readonly MethodInfo _entityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("EntityQuery");

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityQuery<TEntity>(
            QueryContext queryContext,
            IEntityType entityType,
            EntityKeyFactorySource entityKeyFactorySource,
            EntityMaterializerSource entityMaterializerSource,
            InMemoryDatabase inMemoryDatabase,
            bool queryStateManager)
            where TEntity : class
        {
            var keyProperties
                = entityType.GetPrimaryKey().Properties;

            var keyFactory
                = entityKeyFactorySource.GetKeyFactory(keyProperties);

            Func<IValueReader, EntityKey> entityKeyFactory
                = vr => keyFactory.Create(entityType, keyProperties, vr);

            var materializer
                = entityMaterializerSource.GetMaterializer(entityType);

            return inMemoryDatabase
                .GetTable(entityType)
                .Select(t =>
                {
                    var valueReader = new ObjectArrayValueReader(t);
                    var entityKey = entityKeyFactory(valueReader);

                    return (TEntity)queryContext
                        .QueryBuffer
                        .GetEntity(entityType, entityKey, valueReader, materializer, queryStateManager);
                });
        }

        private static readonly MethodInfo _projectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("ProjectionQuery");

        [UsedImplicitly]
        private static IEnumerable<IValueReader> ProjectionQuery(
            IEntityType entityType,
            InMemoryDatabase inMemoryDatabase)
        {
            return inMemoryDatabase.GetTable(entityType).Select(t => new ObjectArrayValueReader(t));
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

            private new InMemoryQueryModelVisitor QueryModelVisitor => (InMemoryQueryModelVisitor)base.QueryModelVisitor;

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                Check.NotNull(elementType, "elementType");

                var entityType
                    = QueryModelVisitor.QueryCompilationContext.Model
                        .GetEntityType(elementType);

                var inMemoryDatabase
                    = ((InMemoryQueryCompilationContext)QueryModelVisitor
                        .QueryCompilationContext)
                        .Database;

                if (QueryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    return Expression.Call(
                        _entityQueryMethodInfo.MakeGenericMethod(elementType),
                        QueryContextParameter,
                        Expression.Constant(entityType),
                        Expression.Constant(QueryModelVisitor.QueryCompilationContext.EntityKeyFactorySource),
                        Expression.Constant(QueryModelVisitor.QueryCompilationContext.EntityMaterializerSource),
                        Expression.Constant(inMemoryDatabase),
                        Expression.Constant(QueryModelVisitor.QuerySourceRequiresTracking(_querySource)));
                }

                return Expression.Call(
                    _projectionQueryMethodInfo,
                    Expression.Constant(entityType),
                    Expression.Constant(inMemoryDatabase));
            }
        }
    }
}
