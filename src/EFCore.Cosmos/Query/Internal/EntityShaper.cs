// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public class EntityShaper : IShaper
    {
        private readonly IEntityType _entityType;
        private readonly bool _trackingQuery;
        private readonly bool _useQueryBuffer;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public EntityShaper(
            IEntityType entityType,
            bool trackingQuery,
            bool useQueryBuffer,
            IEntityMaterializerSource entityMaterializerSource)
        {
            _entityType = entityType;
            _entityMaterializerSource = entityMaterializerSource;
            _trackingQuery = trackingQuery;
            _useQueryBuffer = useQueryBuffer;
        }

        public virtual Type Type => _entityType.ClrType;

        public virtual LambdaExpression CreateShaperLambda()
        {
            var jObjectParameter = Expression.Parameter(typeof(JObject), "jObject");

            var entityInfo = CreateEntityInfoExpression(_entityType, null);

            return Expression.Lambda(
                Expression.Convert(
                    Expression.Call(
                        _shapeMethodInfo,
                        jObjectParameter,
                        EntityQueryModelVisitor.QueryContextParameter,
                        Expression.Constant(_trackingQuery),
                        Expression.Constant(_useQueryBuffer),
                        entityInfo),
                    _entityType.ClrType),
                jObjectParameter);
        }

        private NewExpression CreateEntityInfoExpression(IEntityType entityType, INavigation navigation)
        {
            var valueBufferFactory = ValueBufferFactoryFactory.Create(entityType);

            var materializationContextParameter
                            = Expression.Parameter(typeof(MaterializationContext), "materializationContext");
            var materializer = Expression.Lambda(_entityMaterializerSource
                    .CreateMaterializeExpression(
                        entityType, materializationContextParameter), materializationContextParameter);

            var nestedEntities = new List<Expression>();
            foreach (var ownedNavigation in entityType.GetNavigations())
            {
                var fk = ownedNavigation.ForeignKey;
                if (!fk.IsOwnership
                    || ownedNavigation.IsDependentToPrincipal()
                    || fk.DeclaringEntityType.IsDocumentRoot())
                {
                    continue;
                }

                nestedEntities.Add(CreateEntityInfoExpression(fk.DeclaringEntityType, ownedNavigation));
            }

            var nestedEntitiesExpression = nestedEntities.Count == 0
                ? (Expression)Expression.Constant(null, typeof(IList<EntityInfo>))
                : Expression.ListInit(Expression.New(typeof(List<EntityInfo>)),
                    nestedEntities.Select(n => Expression.ElementInit(_listAddMethodInfo, n)));

            return Expression.New(
                    EntityInfo.ConstructorInfo,
                    Expression.Constant(navigation, typeof(INavigation)),
                    Expression.Constant(entityType.FindPrimaryKey()),
                    valueBufferFactory,
                    materializer,
                    nestedEntitiesExpression);
        }

        private static readonly MethodInfo _listAddMethodInfo
            = typeof(List<EntityInfo>).GetTypeInfo().GetDeclaredMethod(nameof(List<EntityInfo>.Add));

        private static readonly MethodInfo _shapeMethodInfo
            = typeof(EntityShaper).GetTypeInfo().GetDeclaredMethod(nameof(Shape));

        [UsedImplicitly]
        private static object Shape(
            JObject jObject,
            QueryContext queryContext,
            bool trackingQuery,
            bool bufferedQuery,
            EntityInfo entityInfo)
        {
            var valueBuffer = new ValueBuffer(entityInfo.ValueBufferFactory(jObject));

            if (!bufferedQuery)
            {
                if (trackingQuery)
                {
                    var entry = queryContext.StateManager.TryGetEntry(entityInfo.Key, valueBuffer, throwOnNullKey: true);
                    if (entry != null)
                    {
                        return ShapeNestedEntities(
                            jObject,
                            queryContext,
                            trackingQuery,
                            bufferedQuery,
                            entityInfo,
                            entry.Entity);
                    }
                }

                var entity = entityInfo.Materializer(new MaterializationContext(valueBuffer, queryContext.Context));
                return ShapeNestedEntities(
                            jObject,
                            queryContext,
                            trackingQuery,
                            bufferedQuery,
                            entityInfo,
                            entity);
            }
            else
            {
                var entity = queryContext.QueryBuffer
                        .GetEntity(
                            entityInfo.Key,
                            new EntityLoadInfo(
                                new MaterializationContext(valueBuffer, queryContext.Context),
                                entityInfo.Materializer),
                            queryStateManager: trackingQuery,
                            throwOnNullKey: true);

                return ShapeNestedEntities(
                            jObject,
                            queryContext,
                            trackingQuery,
                            bufferedQuery,
                            entityInfo,
                            entity);
            }
        }

        private static object ShapeNestedEntities(
            JObject jObject,
            QueryContext queryContext,
            bool trackingQuery,
            bool bufferedQuery,
            EntityInfo entityInfo,
            object parentEntity)
        {
            if (entityInfo.NestedEntities == null)
            {
                return parentEntity;
            }

            foreach (var nestedEntityInfo in entityInfo.NestedEntities)
            {
                var nestedNavigation = nestedEntityInfo.Navigation;
                if (nestedNavigation.ForeignKey.IsUnique)
                {
                    var nestedJObject = (JObject)jObject[nestedEntityInfo.Navigation.Name];
                    if (nestedJObject == null)
                    {
                        continue;
                    }

                    var nestedEntity = Shape(
                        nestedJObject,
                        queryContext,
                        trackingQuery,
                        bufferedQuery,
                        nestedEntityInfo);
                    nestedNavigation.GetSetter().SetClrValue(parentEntity, nestedEntity);
                }
                else
                {
                    var jArray = (JArray)jObject[nestedEntityInfo.Navigation.Name];
                    var nestedEntities = new List<object>();
                    if (jArray != null
                        && jArray.Count != 0)
                    {
                        foreach(JObject nestedJObject in jArray)
                        {
                            nestedEntities.Add(Shape(
                                nestedJObject,
                                queryContext,
                                trackingQuery,
                                bufferedQuery,
                                nestedEntityInfo));
                        }
                    }

                    nestedNavigation.GetCollectionAccessor().AddRange(parentEntity, nestedEntities);
                }
            }

            return parentEntity;
        }

        private class EntityInfo
        {
            public static readonly ConstructorInfo ConstructorInfo
                = typeof(EntityInfo).GetTypeInfo().DeclaredConstructors.Single(c => c.GetParameters().Length > 0);

            public EntityInfo(
                INavigation navigation,
                IKey key,
                Func<JObject, object[]> valueBufferFactory,
                Func<MaterializationContext, object> materializer,
                IList<EntityInfo> nestedEntities)
            {
                Navigation = navigation;
                Key = key;
                ValueBufferFactory = valueBufferFactory;
                Materializer = materializer;
                NestedEntities = nestedEntities;
            }

            public INavigation Navigation { get; }
            public IKey Key { get; }
            public Func<JObject, object[]> ValueBufferFactory { get; }
            public Func<MaterializationContext, object> Materializer { get; }
            public IList<EntityInfo> NestedEntities { get; }
        }
    }
}
