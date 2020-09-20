// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public partial class CosmosShapedQueryCompilingExpressionVisitor
    {
        private abstract class CosmosProjectionBindingRemovingExpressionVisitorBase : ExpressionVisitor
        {
            private static readonly MethodInfo _getItemMethodInfo
                = typeof(JObject).GetRuntimeProperties()
                    .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                    .GetMethod;

            private static readonly PropertyInfo _jTokenTypePropertyInfo
                = typeof(JToken).GetRuntimeProperties()
                    .Single(mi => mi.Name == nameof(JToken.Type));

            private static readonly MethodInfo _jTokenToObjectMethodInfo
                = typeof(JToken).GetRuntimeMethods()
                    .Single(mi => mi.Name == nameof(JToken.ToObject) && mi.GetParameters().Length == 0);

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

            private static readonly MethodInfo _collectionAccessorGetOrCreateMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate));

            private readonly ParameterExpression _jObjectParameter;
            private readonly bool _trackQueryResults;

            private readonly IDictionary<ParameterExpression, Expression> _materializationContextBindings
                = new Dictionary<ParameterExpression, Expression>();

            private readonly IDictionary<Expression, ParameterExpression> _projectionBindings
                = new Dictionary<Expression, ParameterExpression>();

            private readonly IDictionary<Expression, (IEntityType EntityType, Expression JObjectExpression)> _ownerMappings
                = new Dictionary<Expression, (IEntityType, Expression)>();

            private readonly IDictionary<Expression, Expression> _ordinalParameterBindings
                = new Dictionary<Expression, Expression>();

            private List<IncludeExpression> _pendingIncludes
                = new List<IncludeExpression>();

            private static readonly MethodInfo _toObjectMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase)
                    .GetRuntimeMethods().Single(mi => mi.Name == nameof(SafeToObject));

            public CosmosProjectionBindingRemovingExpressionVisitorBase(
                [NotNull] ParameterExpression jObjectParameter,
                bool trackQueryResults)
            {
                _jObjectParameter = jObjectParameter;
                _trackQueryResults = trackQueryResults;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                Check.NotNull(binaryExpression, nameof(binaryExpression));

                if (binaryExpression.NodeType == ExpressionType.Assign)
                {
                    if (binaryExpression.Left is ParameterExpression parameterExpression)
                    {
                        if (parameterExpression.Type == typeof(JObject)
                            || parameterExpression.Type == typeof(JArray))
                        {
                            string storeName = null;

                            // Values injected by JObjectInjectingExpressionVisitor
                            var projectionExpression = ((UnaryExpression)binaryExpression.Right).Operand;
                            if (projectionExpression is ProjectionBindingExpression projectionBindingExpression)
                            {
                                var projection = GetProjection(projectionBindingExpression);
                                projectionExpression = projection.Expression;
                                storeName = projection.Alias;
                            }
                            else if (projectionExpression is UnaryExpression convertExpression
                                && convertExpression.NodeType == ExpressionType.Convert)
                            {
                                // Unwrap EntityProjectionExpression when the root entity is not projected
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

                                switch (accessExpression)
                                {
                                    case ObjectAccessExpression innerObjectAccessExpression:
                                        innerAccessExpression = innerObjectAccessExpression.AccessExpression;
                                        _ownerMappings[accessExpression] =
                                            (innerObjectAccessExpression.Navigation.DeclaringEntityType, innerAccessExpression);
                                        break;
                                    case RootReferenceExpression _:
                                        innerAccessExpression = _jObjectParameter;
                                        break;
                                    default:
                                        throw new InvalidOperationException(
                                            CoreStrings.QueryFailed(binaryExpression.Print(), GetType().Name));
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

                            var updatedExpression = Expression.New(
                                newExpression.Constructor,
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
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                var method = methodCallExpression.Method;
                var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                if (genericMethod == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
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

                if (method.DeclaringType == typeof(Enumerable)
                    && method.Name == nameof(Enumerable.Select)
                    && genericMethod == EnumerableMethods.Select)
                {
                    var lambda = (LambdaExpression)methodCallExpression.Arguments[1];
                    if (lambda.Body is IncludeExpression includeExpression)
                    {
                        if (!(includeExpression.Navigation is INavigation navigation)
                            || navigation.IsOnDependent
                            || navigation.ForeignKey.DeclaringEntityType.IsDocumentRoot())
                        {
                            throw new InvalidOperationException(
                                CosmosStrings.NonEmbeddedIncludeNotSupported(includeExpression.Navigation));
                        }

                        _pendingIncludes.Add(includeExpression);

                        Visit(includeExpression.EntityExpression);

                        // Includes on collections are processed when visiting CollectionShaperExpression
                        return Visit(methodCallExpression.Arguments[0]);
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

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

                        var accessExpression = objectArrayProjection.InnerProjection.AccessExpression;
                        _projectionBindings[accessExpression] = jObjectParameter;
                        _ownerMappings[accessExpression] =
                            (objectArrayProjection.Navigation.DeclaringEntityType, objectArrayProjection.AccessExpression);
                        _ordinalParameterBindings[accessExpression] = Expression.Add(
                            ordinalParameter, Expression.Constant(1, typeof(int)));

                        var innerShaper = (BlockExpression)Visit(collectionShaperExpression.InnerShaper);

                        innerShaper = AddIncludes(innerShaper);

                        var entities = Expression.Call(
                            EnumerableMethods.SelectWithOrdinal.MakeGenericMethod(typeof(JObject), innerShaper.Type),
                            Expression.Call(
                                EnumerableMethods.Cast.MakeGenericMethod(typeof(JObject)),
                                jArray),
                            Expression.Lambda(innerShaper, jObjectParameter, ordinalParameter));

                        var navigation = collectionShaperExpression.Navigation;
                        return Expression.Call(
                            _populateCollectionMethodInfo.MakeGenericMethod(navigation.TargetEntityType.ClrType, navigation.ClrType),
                            Expression.Constant(navigation.GetCollectionAccessor()),
                            entities);
                    }

                    case IncludeExpression includeExpression:
                    {
                        if (!(includeExpression.Navigation is INavigation navigation)
                            || navigation.IsOnDependent
                            || navigation.ForeignKey.DeclaringEntityType.IsDocumentRoot())
                        {
                            throw new InvalidOperationException(
                                CosmosStrings.NonEmbeddedIncludeNotSupported(includeExpression.Navigation));
                        }

                        var isFirstInclude = _pendingIncludes.Count == 0;
                        _pendingIncludes.Add(includeExpression);

                        var jObjectBlock = Visit(includeExpression.EntityExpression) as BlockExpression;

                        if (!isFirstInclude)
                        {
                            return jObjectBlock;
                        }

                        Check.DebugAssert(jObjectBlock != null, "The first include must end up on a valid shaper block");

                        // These are the expressions added by JObjectInjectingExpressionVisitor
                        var jObjectCondition = (ConditionalExpression)jObjectBlock.Expressions[^1];

                        var shaperBlock = (BlockExpression)jObjectCondition.IfFalse;
                        shaperBlock = AddIncludes(shaperBlock);

                        var jObjectExpressions = new List<Expression>(jObjectBlock.Expressions);
                        jObjectExpressions.RemoveAt(jObjectExpressions.Count - 1);

                        jObjectExpressions.Add(
                            jObjectCondition.Update(jObjectCondition.Test, jObjectCondition.IfTrue, shaperBlock));

                        return jObjectBlock.Update(jObjectBlock.Variables, jObjectExpressions);
                    }
                }

                return base.VisitExtension(extensionExpression);
            }

            private BlockExpression AddIncludes(BlockExpression shaperBlock)
            {
                if (_pendingIncludes.Count == 0)
                {
                    return shaperBlock;
                }

                var shaperExpressions = new List<Expression>(shaperBlock.Expressions);
                var instanceVariable = shaperExpressions[^1];
                shaperExpressions.RemoveAt(shaperExpressions.Count - 1);

                var includesToProcess = _pendingIncludes;
                _pendingIncludes = new List<IncludeExpression>();

                foreach (var include in includesToProcess)
                {
                    AddInclude(shaperExpressions, include, shaperBlock, instanceVariable);
                }

                shaperExpressions.Add(instanceVariable);
                shaperBlock = shaperBlock.Update(shaperBlock.Variables, shaperExpressions);
                return shaperBlock;
            }

            private void AddInclude(
                List<Expression> shaperExpressions,
                IncludeExpression includeExpression,
                BlockExpression shaperBlock,
                Expression instanceVariable)
            {
                // Cosmos does not support Includes for ISkipNavigation
                var navigation = (INavigation)includeExpression.Navigation;
                var includeMethod = navigation.IsCollection ? _includeCollectionMethodInfo : _includeReferenceMethodInfo;
                var includingClrType = navigation.DeclaringEntityType.ClrType;
                var relatedEntityClrType = navigation.TargetEntityType.ClrType;
#pragma warning disable EF1001 // Internal EF Core API usage.
                var entityEntryVariable = _trackQueryResults
                    ? shaperBlock.Variables.Single(v => v.Type == typeof(InternalEntityEntry))
                    : (Expression)Expression.Constant(null, typeof(InternalEntityEntry));
#pragma warning restore EF1001 // Internal EF Core API usage.

                var concreteEntityTypeVariable = shaperBlock.Variables.Single(v => v.Type == typeof(IEntityType));
                var inverseNavigation = navigation.Inverse;
                var fixup = GenerateFixup(
                    includingClrType, relatedEntityClrType, navigation, inverseNavigation);
                var initialize = GenerateInitialize(includingClrType, navigation);

                var navigationExpression = Visit(includeExpression.NavigationExpression);

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
            }

            private static readonly MethodInfo _includeReferenceMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeReference));

            private static void IncludeReference<TIncludingEntity, TIncludedEntity>(
#pragma warning disable EF1001 // Internal EF Core API usage.
                InternalEntityEntry entry,
#pragma warning restore EF1001 // Internal EF Core API usage.
                object entity,
                IEntityType entityType,
                TIncludedEntity relatedEntity,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                Action<TIncludingEntity> _)
            {
                if (entity == null
                    || !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
                {
                    return;
                }

                if (entry == null)
                {
                    var includingEntity = (TIncludingEntity)entity;
                    navigation.SetIsLoadedWhenNoTracking(includingEntity);
                    if (relatedEntity != null)
                    {
                        fixup(includingEntity, relatedEntity);
                        if (inverseNavigation != null
                            && !inverseNavigation.IsCollection)
                        {
                            inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                        }
                    }
                }
                // For non-null relatedEntity StateManager will set the flag
                else if (relatedEntity == null)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    entry.SetIsLoaded(navigation);
#pragma warning restore EF1001 // Internal EF Core API usage.
                }
            }

            private static readonly MethodInfo _includeCollectionMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeCollection));

            private static void IncludeCollection<TIncludingEntity, TIncludedEntity>(
#pragma warning disable EF1001 // Internal EF Core API usage.
                InternalEntityEntry entry,
#pragma warning restore EF1001 // Internal EF Core API usage.
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
                    navigation.SetIsLoadedWhenNoTracking(includingEntity);

                    if (relatedEntities != null)
                    {
                        foreach (var relatedEntity in relatedEntities)
                        {
                            fixup(includingEntity, relatedEntity);
                            if (inverseNavigation != null)
                            {
                                inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
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
#pragma warning disable EF1001 // Internal EF Core API usage.
                    entry.SetIsLoaded(navigation);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    if (relatedEntities != null)
                    {
                        using var enumerator = relatedEntities.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                        }
                    }
                    else
                    {
                        initialize((TIncludingEntity)entity);
                    }
                }
            }

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
                    navigation.IsCollection
                        ? AddToCollectionNavigation(entityParameter, relatedEntityParameter, navigation)
                        : AssignReferenceNavigation(entityParameter, relatedEntityParameter, navigation)
                };

                if (inverseNavigation != null)
                {
                    expressions.Add(
                        inverseNavigation.IsCollection
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
                if (!navigation.IsCollection)
                {
                    return null;
                }

                var entityParameter = Expression.Parameter(entityType);

                var getOrCreateExpression = Expression.Call(
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
                = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase).GetTypeInfo()
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

            protected abstract ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression);

            private static Expression CreateReadJTokenExpression(Expression jObjectExpression, string propertyName)
                => Expression.Call(jObjectExpression, _getItemMethodInfo, Expression.Constant(propertyName));

            private Expression CreateGetValueExpression(
                Expression jObjectExpression,
                IProperty property,
                Type type)
            {
                if (property.Name == StoreKeyConvention.JObjectPropertyName)
                {
                    return _projectionBindings[jObjectExpression];
                }

                var storeName = property.GetJsonPropertyName();
                if (storeName.Length == 0)
                {
                    var entityType = property.DeclaringEntityType;
                    if (!entityType.IsDocumentRoot())
                    {
                        var ownership = entityType.FindOwnership();
                        if (!ownership.IsUnique
                            && property.IsOrdinalKeyProperty())
                        {
                            Expression readExpression = _ordinalParameterBindings[jObjectExpression];
                            if (readExpression.Type != type)
                            {
                                readExpression = Expression.Convert(readExpression, type);
                            }

                            return readExpression;
                        }

                        var principalProperty = property.FindFirstPrincipal();
                        if (principalProperty != null)
                        {
                            Expression ownerJObjectExpression = null;
                            if (_ownerMappings.TryGetValue(jObjectExpression, out var ownerInfo))
                            {
                                Check.DebugAssert(
                                    principalProperty.DeclaringEntityType.IsAssignableFrom(ownerInfo.EntityType),
                                    $"{principalProperty.DeclaringEntityType} is not assignable from {ownerInfo.EntityType}");

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
                                return CreateGetValueExpression(ownerJObjectExpression, principalProperty, type);
                            }
                        }
                    }

                    return Expression.Default(type);
                }

                return Expression.Convert(
                    CreateGetValueExpression(jObjectExpression, storeName, type.MakeNullable(), property.GetTypeMapping()),
                    type);
            }

            private Expression CreateGetValueExpression(
                Expression jObjectExpression,
                string storeName,
                Type type,
                CoreTypeMapping typeMapping = null)
            {
                Check.DebugAssert(type.IsNullableType(), "Must read nullable type from JObject.");

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

                    if (body.Type != type)
                    {
                        body = Expression.Convert(body, type);
                    }

                    body = Expression.Condition(
                        Expression.OrElse(
                            Expression.Equal(jTokenParameter, Expression.Default(typeof(JToken))),
                            Expression.Equal(
                                Expression.MakeMemberAccess(jTokenParameter, _jTokenTypePropertyInfo),
                                Expression.Constant(JTokenType.Null))),
                        Expression.Default(type),
                        body);

                    valueExpression = Expression.Invoke(Expression.Lambda(body, jTokenParameter), jTokenExpression);
                }
                else
                {
                    valueExpression = ConvertJTokenToType(jTokenExpression, typeMapping?.ClrType.MakeNullable() ?? type);

                    if (valueExpression.Type != type)
                    {
                        valueExpression = Expression.Convert(valueExpression, type);
                    }
                }

                return valueExpression;
            }

            private Expression ConvertJTokenToType(Expression jTokenExpression, Type type)
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
