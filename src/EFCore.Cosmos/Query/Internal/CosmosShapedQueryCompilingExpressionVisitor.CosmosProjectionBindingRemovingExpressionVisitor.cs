// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public partial class CosmosShapedQueryCompilingExpressionVisitor
    {
        private class CosmosProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            private static readonly MethodInfo _selectMethodInfo
                = typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(nameof(Enumerable.Select))
                    .Single(mi => mi.GetParameters().Length == 2 && mi.GetParameters()[1].ParameterType.GetGenericArguments().Length == 3);
            private static readonly MethodInfo _castMethodInfo
                = typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(nameof(Enumerable.Cast))
                    .Single(mi => mi.GetParameters().Length == 1);
            private static readonly MethodInfo _getItemMethodInfo
                = typeof(JObject).GetTypeInfo().GetRuntimeProperties()
                    .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                    .GetMethod;
            private static readonly PropertyInfo _jTokenTypePropertyInfo
                = typeof(JToken).GetTypeInfo().GetRuntimeProperties()
                    .Single(mi => mi.Name == nameof(JToken.Type));
            private static readonly MethodInfo _jTokenToObjectMethodInfo
                = typeof(JToken).GetTypeInfo().GetRuntimeMethods()
                    .Single(mi => mi.Name == nameof(JToken.ToObject) && mi.GetParameters().Length == 0);
            private static readonly MethodInfo _toObjectMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitor).GetTypeInfo().GetRuntimeMethods()
                    .Single(mi => mi.Name == nameof(SafeToObject));
            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));
            private static readonly MethodInfo _collectionAccessorGetOrCreateMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate));

            private readonly SelectExpression _selectExpression;
            private readonly ParameterExpression _jObjectParameter;
            private readonly bool _trackQueryResults;

            private readonly IDictionary<ParameterExpression, Expression> _materializationContextBindings
                = new Dictionary<ParameterExpression, Expression>();
            private readonly IDictionary<Expression, ParameterExpression> _projectionBindings
                = new Dictionary<Expression, ParameterExpression>();
            private readonly IDictionary<Expression, (IEntityType EntityType, Expression JObjectExpression)> _ownerMappings
                = new Dictionary<Expression, (IEntityType, Expression)>();
            private (IEntityType EntityType, ParameterExpression JObjectVariable) _ownerInfo;
            private ParameterExpression _ordinalParameter;

            public CosmosProjectionBindingRemovingExpressionVisitor(
                SelectExpression selectExpression,
                ParameterExpression jObjectParameter,
                bool trackQueryResults)
            {
                _selectExpression = selectExpression;
                _jObjectParameter = jObjectParameter;
                _trackQueryResults = trackQueryResults;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Assign)
                {
                    if (binaryExpression.Left is ParameterExpression parameterExpression)
                    {
                        if (parameterExpression.Type == typeof(JObject)
                            || parameterExpression.Type == typeof(JArray))
                        {
                            string storeName = null;
                            var projectionExpression = ((UnaryExpression)binaryExpression.Right).Operand;
                            if (projectionExpression is ProjectionBindingExpression projectionBindingExpression)
                            {
                                var projection = GetProjection(projectionBindingExpression);
                                projectionExpression = projection.Expression;
                                storeName = projection.Alias;
                            } else if (projectionExpression is UnaryExpression convertExpression)
                            {
                                projectionExpression = ((UnaryExpression)convertExpression.Operand).Operand;
                            }

                            Expression innerAccessExpression;
                            if (projectionExpression is ObjectArrayProjectionExpression objectArrayProjectionExpression)
                            {
                                innerAccessExpression = objectArrayProjectionExpression.AccessExpression;
                                _projectionBindings[objectArrayProjectionExpression] = parameterExpression;
                                storeName ??= objectArrayProjectionExpression.Name;
                            }
                            else
                            {
                                var entityProjectionExpression = (EntityProjectionExpression)projectionExpression;
                                var accessExpression = entityProjectionExpression.AccessExpression;
                                _projectionBindings[accessExpression] = parameterExpression;
                                storeName ??= entityProjectionExpression.Name;
                                if (_ownerInfo.EntityType != null)
                                {
                                    _ownerMappings[accessExpression] = _ownerInfo;
                                }

                                switch (accessExpression)
                                {
                                    case ObjectAccessExpression innerObjectAccessExpression:
                                        innerAccessExpression = innerObjectAccessExpression.AccessExpression;
                                        break;
                                    case RootReferenceExpression _:
                                        innerAccessExpression = _jObjectParameter;
                                        break;
                                    default:
                                        throw new InvalidOperationException(CoreStrings.QueryFailed(binaryExpression.Print(), GetType().Name));
                                }
                            }

                            var valueExpression = CreateGetValueExpression(innerAccessExpression, storeName, parameterExpression.Type);

                            return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, valueExpression);
                        }

                        if (parameterExpression.Type == typeof(MaterializationContext))
                        {
                            var newExpression = (NewExpression)binaryExpression.Right;

                            EntityProjectionExpression entityProjectionExpression;
                            if (newExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                            {
                                var projection = GetProjection(projectionBindingExpression);
                                entityProjectionExpression = (EntityProjectionExpression)projection.Expression;
                            }
                            else
                            {
                                var projection = ((UnaryExpression)((UnaryExpression)newExpression.Arguments[0]).Operand).Operand;
                                entityProjectionExpression = (EntityProjectionExpression)projection;
                            }

                            _materializationContextBindings[parameterExpression] = entityProjectionExpression.AccessExpression;

                            var updatedExpression = Expression.New(newExpression.Constructor,
                                Expression.Constant(ValueBuffer.Empty),
                                newExpression.Arguments[1]);

                            return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                        }
                    }

                    if (binaryExpression.Left is MemberExpression memberExpression
                        && memberExpression.Member is FieldInfo fieldInfo
                        && fieldInfo.IsInitOnly)
                    {
                        return memberExpression.Assign(Visit(binaryExpression.Right));
                    }
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
                {
                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                    Expression innerExpression;
                    if (methodCallExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                    {
                        var projection = GetProjection(projectionBindingExpression);

                        innerExpression = Expression.Convert(
                            CreateReadJTokenExpression(_jObjectParameter, projection.Alias),
                            typeof(JObject));
                    }
                    else
                    {
                        innerExpression = _materializationContextBindings[
                            (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];
                    }

                    return CreateGetValueExpression(innerExpression, property, methodCallExpression.Type);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                switch (extensionExpression)
                {
                    case ProjectionBindingExpression projectionBindingExpression:
                    {
                        var projection = GetProjection(projectionBindingExpression);

                        return CreateGetValueExpression(
                            _jObjectParameter,
                            projection.Alias,
                            projectionBindingExpression.Type, (projection.Expression as SqlExpression)?.TypeMapping);
                    }

                    case CollectionShaperExpression collectionShaperExpression:
                    {
                        ObjectArrayProjectionExpression objectArrayProjection;
                        switch (collectionShaperExpression.Projection)
                        {
                            case ProjectionBindingExpression projectionBindingExpression:
                                var projection = GetProjection(projectionBindingExpression);
                                objectArrayProjection = (ObjectArrayProjectionExpression)projection.Expression;
                                break;
                            case ObjectArrayProjectionExpression objectArrayProjectionExpression:
                                objectArrayProjection = objectArrayProjectionExpression;
                                break;
                            default:
                                throw new InvalidOperationException(CoreStrings.QueryFailed(extensionExpression.Print(), GetType().Name));
                        }

                        var jArray = _projectionBindings[objectArrayProjection];
                        var jObjectParameter = Expression.Parameter(typeof(JObject), jArray.Name + "Object");
                        var ordinalParameter = Expression.Parameter(typeof(int), jArray.Name + "Ordinal");

                        _projectionBindings[objectArrayProjection.InnerProjection.AccessExpression] = jObjectParameter;
                        if (_ownerInfo.EntityType != null)
                        {
                            _ownerMappings[objectArrayProjection.InnerProjection.AccessExpression] = _ownerInfo;
                        }
                        else
                        {
                            _ownerMappings[objectArrayProjection.InnerProjection.AccessExpression] = (objectArrayProjection.Navigation.DeclaringEntityType, objectArrayProjection.AccessExpression);
                        }

                        var previousOrdinalParameter = _ordinalParameter;
                        _ordinalParameter = ordinalParameter;
                        var innerShaper = Visit(collectionShaperExpression.InnerShaper);
                        _ordinalParameter = previousOrdinalParameter;

                        var entities = Expression.Call(
                            _selectMethodInfo.MakeGenericMethod(typeof(JObject), innerShaper.Type),
                            Expression.Call(
                                _castMethodInfo.MakeGenericMethod(typeof(JObject)),
                                jArray),
                            Expression.Lambda(innerShaper, jObjectParameter, ordinalParameter));

                        var navigation = collectionShaperExpression.Navigation;
                        return Expression.Call(
                            _populateCollectionMethodInfo.MakeGenericMethod(navigation.GetTargetType().ClrType, navigation.ClrType),
                            Expression.Constant(navigation.GetCollectionAccessor()),
                            entities);
                    }

                    case IncludeExpression includeExpression:
                    {
                        var navigation = includeExpression.Navigation;
                        var fk = navigation.ForeignKey;
                        if (includeExpression.Navigation.IsDependentToPrincipal()
                            || fk.DeclaringEntityType.IsDocumentRoot())
                        {
                            throw new InvalidOperationException(
                                "Non-embedded IncludeExpression " + includeExpression.Print());
                        }

                        // These are the expressions added by JObjectInjectingExpressionVisitor
                        var jObjectBlock = (BlockExpression)Visit(includeExpression.EntityExpression);
                        var jObjectVariable = jObjectBlock.Variables.Single(v => v.Type == typeof(JObject));
                        var jObjectCondition = (ConditionalExpression)jObjectBlock.Expressions[jObjectBlock.Expressions.Count - 1];

                        var shaperBlock = (BlockExpression)jObjectCondition.IfFalse;
                        var shaperExpressions = new List<Expression>(shaperBlock.Expressions);
                        var instanceVariable = shaperExpressions[shaperExpressions.Count - 1];
                        shaperExpressions.RemoveAt(shaperExpressions.Count - 1);

                        var includeMethod = navigation.IsCollection() ? _includeCollectionMethodInfo : _includeReferenceMethodInfo;
                        var includingClrType = navigation.DeclaringEntityType.ClrType;
                        var relatedEntityClrType = navigation.GetTargetType().ClrType;
                        var entityEntryVariable = _trackQueryResults
                            ? shaperBlock.Variables.Single(v => v.Type == typeof(InternalEntityEntry))
                            : (Expression)Expression.Constant(null, typeof(InternalEntityEntry));
                        var concreteEntityTypeVariable = shaperBlock.Variables.Single(v => v.Type == typeof(IEntityType));
                        var inverseNavigation = navigation.FindInverse();
                        var fixup = GenerateFixup(
                            includingClrType, relatedEntityClrType, navigation, inverseNavigation);
                        var initialize = GenerateInitialize(includingClrType, navigation);

                        var previousOwner = _ownerInfo;
                        _ownerInfo = (navigation.DeclaringEntityType, jObjectVariable);
                        var navigationExpression = Visit(includeExpression.NavigationExpression);
                        _ownerInfo = previousOwner;

                        shaperExpressions.Add(
                            Expression.Call(
                                includeMethod.MakeGenericMethod(includingClrType, relatedEntityClrType),
                                entityEntryVariable,
                                instanceVariable,
                                concreteEntityTypeVariable,
                                navigationExpression,
                                Expression.Constant(navigation),
                                Expression.Constant(inverseNavigation, typeof(INavigation)),
                                Expression.Constant(fixup),
                                Expression.Constant(initialize, typeof(Action<>).MakeGenericType(includingClrType))));

                        shaperExpressions.Add(instanceVariable);
                        shaperBlock = shaperBlock.Update(shaperBlock.Variables, shaperExpressions);

                        var jObjectExpressions = new List<Expression>(jObjectBlock.Expressions);
                        jObjectExpressions.RemoveAt(jObjectExpressions.Count - 1);

                        jObjectExpressions.Add(
                            jObjectCondition.Update(jObjectCondition.Test, jObjectCondition.IfTrue, shaperBlock));

                        return jObjectBlock.Update(jObjectBlock.Variables, jObjectExpressions);
                    }
                }

                return base.VisitExtension(extensionExpression);
            }

            private static readonly MethodInfo _includeReferenceMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeReference));

            private static void IncludeReference<TIncludingEntity, TIncludedEntity>(
                InternalEntityEntry entry,
                object entity,
                IEntityType entityType,
                TIncludedEntity relatedEntity,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                Action<TIncludingEntity> initialize)
            {
                if (entity == null
                    || !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
                {
                    return;
                }

                if (entry == null)
                {
                    var includingEntity = (TIncludingEntity)entity;
                    SetIsLoadedNoTracking(includingEntity, navigation);
                    if (relatedEntity != null)
                    {
                        fixup(includingEntity, relatedEntity);
                        if (inverseNavigation != null
                            && !inverseNavigation.IsCollection())
                        {
                            SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                        }
                    }
                }
                // For non-null relatedEntity StateManager will set the flag
                else if (relatedEntity == null)
                {
                    entry.SetIsLoaded(navigation);
                }
            }

            private static readonly MethodInfo _includeCollectionMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeCollection));

            private static void IncludeCollection<TIncludingEntity, TIncludedEntity>(
                InternalEntityEntry entry,
                object entity,
                IEntityType entityType,
                IEnumerable<TIncludedEntity> relatedEntities,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                Action<TIncludingEntity> initialize)
            {
                if (entity == null
                    || !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
                {
                    return;
                }

                if (entry == null)
                {
                    var includingEntity = (TIncludingEntity)entity;
                    SetIsLoadedNoTracking(includingEntity, navigation);

                    if (relatedEntities != null)
                    {
                        foreach (var relatedEntity in relatedEntities)
                        {
                            fixup(includingEntity, relatedEntity);
                            if (inverseNavigation != null)
                            {
                                SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                            }
                        }
                    }
                    else
                    {
                        initialize(includingEntity);
                    }
                }
                else
                {
                    entry.SetIsLoaded(navigation);
                    if (relatedEntities != null)
                    {
                        using (var enumerator = relatedEntities.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                            }
                        }
                    }
                    else
                    {
                        initialize((TIncludingEntity)entity);
                    }
                }
            }

            private static void SetIsLoadedNoTracking(object entity, INavigation navigation)
                => ((ILazyLoader)(navigation
                            .DeclaringEntityType
                            .GetServiceProperties()
                            .FirstOrDefault(p => p.ClrType == typeof(ILazyLoader)))
                        ?.GetGetter().GetClrValue(entity))
                    ?.SetLoaded(entity, navigation.Name);

            private static Delegate GenerateFixup(
                Type entityType,
                Type relatedEntityType,
                INavigation navigation,
                INavigation inverseNavigation)
            {
                var entityParameter = Expression.Parameter(entityType);
                var relatedEntityParameter = Expression.Parameter(relatedEntityType);
                var expressions = new List<Expression>
                {
                    navigation.IsCollection()
                        ? AddToCollectionNavigation(entityParameter, relatedEntityParameter, navigation)
                        : AssignReferenceNavigation(entityParameter, relatedEntityParameter, navigation)
                };

                if (inverseNavigation != null)
                {
                    expressions.Add(
                        inverseNavigation.IsCollection()
                            ? AddToCollectionNavigation(relatedEntityParameter, entityParameter, inverseNavigation)
                            : AssignReferenceNavigation(relatedEntityParameter, entityParameter, inverseNavigation));

                }

                return Expression.Lambda(Expression.Block(typeof(void), expressions), entityParameter, relatedEntityParameter)
                    .Compile();
            }

            private static Delegate GenerateInitialize(
                Type entityType,
                INavigation navigation)
            {
                if (!navigation.IsCollection())
                {
                    return null;
                }

                var entityParameter = Expression.Parameter(entityType);

                var getOrCreateExpression =  Expression.Call(
                    Expression.Constant(navigation.GetCollectionAccessor()),
                    _collectionAccessorGetOrCreateMethodInfo,
                    entityParameter,
                    Expression.Constant(true));

                return Expression.Lambda(Expression.Block(typeof(void), getOrCreateExpression), entityParameter)
                    .Compile();
            }

            private static Expression AssignReferenceNavigation(
                ParameterExpression entity,
                ParameterExpression relatedEntity,
                INavigation navigation)
                => entity.MakeMemberAccess(navigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(relatedEntity);

            private static Expression AddToCollectionNavigation(
                ParameterExpression entity,
                ParameterExpression relatedEntity,
                INavigation navigation)
                => Expression.Call(
                    Expression.Constant(navigation.GetCollectionAccessor()),
                    _collectionAccessorAddMethodInfo,
                    entity,
                    relatedEntity,
                    Expression.Constant(true));

            private static readonly MethodInfo _populateCollectionMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(PopulateCollection));

            private static TCollection PopulateCollection<TEntity, TCollection>(
                IClrCollectionAccessor accessor,
                IEnumerable<TEntity> entities)
            {
                // TODO: throw a better exception for non ICollection navigations
                var collection = (ICollection<TEntity>)accessor.Create();
                foreach (var entity in entities)
                {
                    collection.Add(entity);
                }

                return (TCollection)collection;
            }

            private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
                => projectionBindingExpression.ProjectionMember != null
                    ? (int)((ConstantExpression)_selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : projectionBindingExpression.Index
                      ?? throw new InvalidOperationException(CoreStrings.QueryFailed(projectionBindingExpression.Print(), GetType().Name));

            private ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            {
                var index = GetProjectionIndex(projectionBindingExpression);
                return _selectExpression.Projection[index];
            }

            private static Expression CreateReadJTokenExpression(Expression jObjectExpression, string propertyName)
                => Expression.Call(jObjectExpression, _getItemMethodInfo, Expression.Constant(propertyName));

            private Expression CreateGetValueExpression(
                Expression jObjectExpression,
                IProperty property,
                Type clrType)
            {
                if (property.Name == StoreKeyConvention.JObjectPropertyName)
                {
                    return _projectionBindings[jObjectExpression];
                }

                var storeName = property.GetPropertyName();
                if (storeName.Length == 0)
                {
                    var entityType = property.DeclaringEntityType;
                    if (!entityType.IsDocumentRoot())
                    {
                        var ownership = entityType.FindOwnership();

                        if (ownership != null
                            && !ownership.IsUnique
                            && property.IsPrimaryKey()
                            && !property.IsForeignKey()
                            && property.ClrType == typeof(int))
                        {
                            Expression readExpression = _ordinalParameter;
                            if (readExpression.Type != clrType)
                            {
                                readExpression = Expression.Convert(readExpression, clrType);
                            }

                            return readExpression;
                        }

                        var principalProperty = property.FindFirstPrincipal();
                        if (principalProperty != null)
                        {
                            Expression ownerJObjectExpression = null;
                            if (_ownerMappings.TryGetValue(jObjectExpression, out var ownerInfo))
                            {
                                Debug.Assert(principalProperty.DeclaringEntityType.IsAssignableFrom(ownerInfo.EntityType));

                                ownerJObjectExpression = ownerInfo.JObjectExpression;
                            }
                            else if (jObjectExpression is RootReferenceExpression rootReferenceExpression)
                            {
                                ownerJObjectExpression = rootReferenceExpression;
                            }
                            else if (jObjectExpression is ObjectAccessExpression objectAccessExpression)
                            {
                                ownerJObjectExpression = objectAccessExpression.AccessExpression;
                            }

                            if (ownerJObjectExpression != null)
                            {
                                return CreateGetValueExpression(ownerJObjectExpression, principalProperty, clrType);
                            }
                        }
                    }

                    return Expression.Default(clrType);
                }

                return CreateGetValueExpression(jObjectExpression, storeName, clrType, property.GetTypeMapping());
            }

            private Expression CreateGetValueExpression(
                Expression jObjectExpression,
                string storeName,
                Type clrType,
                CoreTypeMapping typeMapping = null)
            {
                var innerExpression = jObjectExpression;
                if (_projectionBindings.TryGetValue(jObjectExpression, out var innerVariable))
                {
                    innerExpression = innerVariable;
                }
                else if (jObjectExpression is RootReferenceExpression rootReferenceExpression)
                {
                    innerExpression = CreateGetValueExpression(
                        _jObjectParameter, rootReferenceExpression.Alias, typeof(JObject));
                }
                else if (jObjectExpression is ObjectAccessExpression objectAccessExpression)
                {
                    var innerAccessExpression = objectAccessExpression.AccessExpression;

                    innerExpression = CreateGetValueExpression(
                        innerAccessExpression, ((IAccessExpression)innerAccessExpression).Name, typeof(JObject));
                }

                var jTokenExpression = CreateReadJTokenExpression(innerExpression, storeName);

                Expression valueExpression;
                var converter = typeMapping?.Converter;
                if (converter != null)
                {
                    var jTokenParameter = Expression.Parameter(typeof(JToken));

                    var body
                        = ReplacingExpressionVisitor.Replace(
                            converter.ConvertFromProviderExpression.Parameters.Single(),
                            Expression.Call(
                                jTokenParameter,
                                _jTokenToObjectMethodInfo.MakeGenericMethod(converter.ProviderClrType)),
                            converter.ConvertFromProviderExpression.Body);

                    if (body.Type != clrType)
                    {
                        body = Expression.Convert(body, clrType);
                    }

                    body = Expression.Condition(
                            Expression.OrElse(
                                Expression.Equal(jTokenParameter, Expression.Default(typeof(JToken))),
                                Expression.Equal(Expression.MakeMemberAccess(jTokenParameter, _jTokenTypePropertyInfo),
                                    Expression.Constant(JTokenType.Null))),
                            Expression.Default(clrType),
                            body);

                    valueExpression = Expression.Invoke(Expression.Lambda(body, jTokenParameter), jTokenExpression);
                }
                else
                {
                    valueExpression = ConvertJTokenToType(jTokenExpression, typeMapping?.ClrType.MakeNullable() ?? clrType);

                    if (valueExpression.Type != clrType)
                    {
                        valueExpression = Expression.Convert(valueExpression, clrType);
                    }
                }

                return valueExpression;
            }

            private static Expression ConvertJTokenToType(Expression jTokenExpression, Type type)
                => type == typeof(JToken)
                    ? jTokenExpression
                    : Expression.Call(
                        _toObjectMethodInfo.MakeGenericMethod(type),
                        jTokenExpression);

            private static T SafeToObject<T>(JToken token)
                => token == null || token.Type == JTokenType.Null ? default : token.ToObject<T>();
        }
    }
}
