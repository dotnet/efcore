// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
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
            var usedProperties = new List<IProperty>();
            var materializer = CreateMaterializerExpression(entityType, usedProperties, out var indexMap);

            var valueBufferFactory = ValueBufferFactoryFactory.Create(usedProperties);

            var nestedEntities = new List<Expression>();
            foreach (var ownedNavigation in entityType.GetNavigations().Concat(entityType.GetDerivedNavigations()))
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
                    Expression.Constant(entityType.FindPrimaryKey(), typeof(IKey)),
                    valueBufferFactory,
                    materializer,
                    Expression.Constant(indexMap, typeof(Dictionary<Type, int[]>)),
                    nestedEntitiesExpression);
        }

        private LambdaExpression CreateMaterializerExpression(
            IEntityType entityType,
            List<IProperty> usedProperties,
            out Dictionary<Type, int[]> typeIndexMap)
        {
            typeIndexMap = null;

            var materializationContextParameter
                                        = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

            var concreteEntityTypes = entityType.GetConcreteTypesInHierarchy().ToList();
            var firstEntityType = concreteEntityTypes[0];
            var indexMap = new int[firstEntityType.PropertyCount()];

            foreach (var property in firstEntityType.GetProperties())
            {
                usedProperties.Add(property);
                indexMap[property.GetIndex()] = usedProperties.Count - 1;
            }

            var materializer
                = _entityMaterializerSource
                    .CreateMaterializeExpression(
                        firstEntityType, materializationContextParameter);

            if (concreteEntityTypes.Count == 1)
            {
                return Expression.Lambda(materializer, materializationContextParameter);
            }

            var discriminatorProperty = firstEntityType.Cosmos().DiscriminatorProperty;

            var firstDiscriminatorValue
                = Expression.Constant(
                    firstEntityType.Cosmos().DiscriminatorValue,
                    discriminatorProperty.ClrType);

            var discriminatorValueVariable
                = Expression.Variable(discriminatorProperty.ClrType);

            var returnLabelTarget = Expression.Label(entityType.ClrType);

            var blockExpressions
                = new Expression[]
                {
                    Expression.Assign(
                        discriminatorValueVariable,
                        _entityMaterializerSource
                            .CreateReadValueExpression(
                                Expression.Call(materializationContextParameter, MaterializationContext.GetValueBufferMethod),
                                discriminatorProperty.ClrType,
                                indexMap[discriminatorProperty.GetIndex()],
                                discriminatorProperty)),
                    Expression.IfThenElse(
                        Expression.Equal(discriminatorValueVariable, firstDiscriminatorValue),
                        Expression.Return(returnLabelTarget, materializer),
                        Expression.Throw(
                            Expression.Call(
                                _createUnableToDiscriminateException,
                                Expression.Constant(firstEntityType)))),
                    Expression.Label(
                        returnLabelTarget,
                        Expression.Default(returnLabelTarget.Type))
                };

            foreach (var concreteEntityType in concreteEntityTypes.Skip(1))
            {
                indexMap = new int[concreteEntityType.PropertyCount()];

                var shadowPropertyExists = false;

                foreach (var property in concreteEntityType.GetProperties())
                {
                    var propertyIndex = usedProperties.IndexOf(property);
                    if (propertyIndex == -1)
                    {
                        usedProperties.Add(property);
                        propertyIndex = usedProperties.Count - 1;
                    }
                    indexMap[property.GetIndex()] = propertyIndex;

                    shadowPropertyExists = shadowPropertyExists || property.IsShadowProperty;
                }

                if (shadowPropertyExists)
                {
                    if (typeIndexMap == null)
                    {
                        typeIndexMap = new Dictionary<Type, int[]>();
                    }

                    typeIndexMap[concreteEntityType.ClrType] = indexMap;
                }

                var discriminatorValue
                    = Expression.Constant(
                        concreteEntityType.Cosmos().DiscriminatorValue,
                        discriminatorProperty.ClrType);

                materializer
                    = _entityMaterializerSource
                        .CreateMaterializeExpression(
                            concreteEntityType, materializationContextParameter);

                blockExpressions[1]
                    = Expression.IfThenElse(
                        Expression.Equal(discriminatorValueVariable, discriminatorValue),
                        Expression.Return(returnLabelTarget, materializer),
                        blockExpressions[1]);
            }

            return Expression.Lambda(
                Expression.Block(new[] { discriminatorValueVariable }, blockExpressions),
                materializationContextParameter);
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
                                entityInfo.Materializer,
                                entityInfo.TypeIndexMap),
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

        private static readonly MethodInfo _createUnableToDiscriminateException
            = typeof(EntityShaper).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

        [UsedImplicitly]
        private static Exception CreateUnableToDiscriminateException(IEntityType entityType)
            => new InvalidOperationException(CosmosStrings.UnableToDiscriminate(entityType.DisplayName()));

        private class EntityInfo
        {
            public static readonly ConstructorInfo ConstructorInfo
                = typeof(EntityInfo).GetTypeInfo().DeclaredConstructors.Single(c => c.GetParameters().Length > 0);

            public EntityInfo(
                INavigation navigation,
                IKey key,
                Func<JObject, object[]> valueBufferFactory,
                Func<MaterializationContext, object> materializer,
                Dictionary<Type, int[]> typeIndexMap,
                IList<EntityInfo> nestedEntities)
            {
                Navigation = navigation;
                Key = key;
                ValueBufferFactory = valueBufferFactory;
                Materializer = materializer;
                TypeIndexMap = typeIndexMap;
                NestedEntities = nestedEntities;
            }

            public INavigation Navigation { get; }
            public IKey Key { get; }
            public Func<JObject, object[]> ValueBufferFactory { get; }
            public Func<MaterializationContext, object> Materializer { get; }
            public Dictionary<Type, int[]> TypeIndexMap { get; }
            public IList<EntityInfo> NestedEntities { get; }
        }
    }
}
