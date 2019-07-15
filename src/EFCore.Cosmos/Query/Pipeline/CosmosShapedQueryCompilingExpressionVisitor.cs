// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public CosmosShapedQueryCompilingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            IEntityMaterializerSource entityMaterializerSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory)
            : base(queryCompilationContext, entityMaterializerSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _contextType = queryCompilationContext.ContextType;
            _logger = queryCompilationContext.Logger;
        }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            selectExpression.ApplyProjection();
            var jObjectParameter = Expression.Parameter(typeof(JObject), "jObject");

            var shaperBody = shapedQueryExpression.ShaperExpression;
            shaperBody = new JObjectInjectingExpressionVisitor()
                .Visit(shaperBody);
            shaperBody = InjectEntityMaterializers(shaperBody);
            shaperBody = new CosmosProjectionBindingRemovingExpressionVisitor(selectExpression, jObjectParameter, TrackQueryResults)
                .Visit(shaperBody);

            var shaperLambda = Expression.Lambda(
                shaperBody,
                QueryCompilationContext.QueryContextParameter,
                jObjectParameter);

            return Expression.New(
                (Async
                    ? typeof(AsyncQueryingEnumerable<>)
                    : typeof(QueryingEnumerable<>)).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(CosmosQueryContext)),
                Expression.Constant(_sqlExpressionFactory),
                Expression.Constant(_querySqlGeneratorFactory),
                Expression.Constant(selectExpression),
                Expression.Constant(shaperLambda.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(_logger));
        }

        private class JObjectInjectingExpressionVisitor : ExpressionVisitor
        {
            private int _currentEntityIndex;

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                switch (extensionExpression)
                {
                    case EntityShaperExpression shaperExpression:
                    {
                        _currentEntityIndex++;

                        var valueBufferExpression = shaperExpression.ValueBufferExpression;

                        var jObjectVariable = Expression.Variable(
                            typeof(JObject),
                            "jObject" + _currentEntityIndex);
                        var variables = new List<ParameterExpression>
                        {
                            jObjectVariable
                        };

                        var expressions = new List<Expression>
                        {
                            Expression.Assign(
                                jObjectVariable,
                                Expression.TypeAs(
                                    valueBufferExpression,
                                    typeof(JObject))),
                            Expression.Condition(
                                Expression.Equal(jObjectVariable, Expression.Constant(null, jObjectVariable.Type)),
                                Expression.Constant(null, shaperExpression.Type),
                                shaperExpression)
                        };

                        return Expression.Block(
                            shaperExpression.Type,
                            variables,
                            expressions);
                    }

                    case CollectionShaperExpression collectionShaperExpression:
                    {
                        _currentEntityIndex++;

                        var resultType = typeof(IEnumerable<>).MakeGenericType(collectionShaperExpression.ElementType);

                        var jArrayVariable = Expression.Variable(
                            typeof(JArray),
                            "jArray" + _currentEntityIndex);
                        var variables = new List<ParameterExpression>
                        {
                            jArrayVariable
                        };

                        var expressions = new List<Expression>
                        {
                            Expression.Assign(
                                jArrayVariable,
                                Expression.TypeAs(
                                    collectionShaperExpression.Projection,
                                    typeof(JArray))),

                            Expression.Condition(
                                Expression.Equal(jArrayVariable, Expression.Constant(null, jArrayVariable.Type)),
                                Expression.Constant(null, resultType),
                                Expression.Convert(collectionShaperExpression, resultType))
                        };

                        return Expression.Block(
                            resultType,
                            variables,
                            expressions);
                        }
                }

                return base.VisitExtension(extensionExpression);
            }
        }

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
            private static readonly MethodInfo _toObjectMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitor).GetTypeInfo().GetRuntimeMethods()
                    .Single(mi => mi.Name == nameof(SafeToObject));
            private static readonly MethodInfo _isNullMethodInfo
                = typeof(CosmosProjectionBindingRemovingExpressionVisitor).GetTypeInfo().GetRuntimeMethods()
                    .Single(mi => mi.Name == nameof(IsNull));

            private readonly SelectExpression _selectExpression;
            private readonly ParameterExpression _jObjectParameter;
            private readonly bool _trackQueryResults;

            private readonly IDictionary<ParameterExpression, ParameterExpression> _materializationContextBindings
                = new Dictionary<ParameterExpression, ParameterExpression>();
            private readonly IDictionary<Expression, ParameterExpression> _projectionBindings
                = new Dictionary<Expression, ParameterExpression>();

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

                            Expression accessExpression;
                            if (projectionExpression is ObjectArrayProjectionExpression objectArrayProjectionExpression)
                            {
                                accessExpression = objectArrayProjectionExpression.AccessExpression;
                                _projectionBindings[objectArrayProjectionExpression] = parameterExpression;
                                storeName ??= objectArrayProjectionExpression.Name;
                            }
                            else
                            {
                                var entityProjectionExpression = (EntityProjectionExpression)projectionExpression;
                                _projectionBindings[entityProjectionExpression.AccessExpression] = parameterExpression;
                                storeName ??= entityProjectionExpression.Name;
                                switch (entityProjectionExpression.AccessExpression)
                                {
                                    case ObjectAccessExpression innerObjectAccessExpression:
                                        accessExpression = innerObjectAccessExpression.AccessExpression;
                                        break;
                                    case RootReferenceExpression _:
                                        accessExpression = _jObjectParameter;
                                        break;
                                    default:
                                        throw new InvalidOperationException();
                                }
                            }

                            var valueExpression = CreateGetStoreValueExpression(accessExpression, storeName, parameterExpression.Type);

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

                            _materializationContextBindings[parameterExpression]
                                = _projectionBindings[entityProjectionExpression.AccessExpression];

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

                    var readExpression = CreateGetValueExpression(innerExpression, property);
                    if (readExpression.Type.IsValueType
                        && methodCallExpression.Type == typeof(object))
                    {
                        readExpression = Expression.Convert(readExpression, typeof(object));
                    }

                    return readExpression;
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

                        return CreateGetStoreValueExpression(
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
                            case ObjectArrayProjectionExpression arrayProjectionExpression:
                                objectArrayProjection = arrayProjectionExpression;
                                break;
                            default:
                                throw new InvalidOperationException();
                        }

                        var jArray = _projectionBindings[objectArrayProjection];
                        var jObjectParameter = Expression.Parameter(typeof(JObject), jArray.Name + "object");
                        var ordinalParameter = Expression.Parameter(typeof(int), "ordinal");

                        _projectionBindings[objectArrayProjection.InnerProjection.AccessExpression] = jObjectParameter;

                        var innerShaper = Visit(collectionShaperExpression.InnerShaper);
                        return Expression.Call(
                            _selectMethodInfo.MakeGenericMethod(typeof(JObject), innerShaper.Type),
                            Expression.Call(
                                _castMethodInfo.MakeGenericMethod(typeof(JObject)),
                                jArray),
                            Expression.Lambda(innerShaper, jObjectParameter, ordinalParameter));
                    }

                    case IncludeExpression includeExpression:
                        var navigation = includeExpression.Navigation;
                        var fk = navigation.ForeignKey;
                        if (includeExpression.Navigation.IsDependentToPrincipal()
                            || fk.DeclaringEntityType.IsDocumentRoot())
                        {
                            throw new InvalidOperationException("Non-embedded IncludeExpression " + new ExpressionPrinter().Print(includeExpression));
                        }

                        // These are the expressions added by JObjectInjectingExpressionVisitor
                        var jObjectBlock = (BlockExpression)Visit(includeExpression.EntityExpression);
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
                                includingClrType, relatedEntityClrType, navigation, inverseNavigation)
                            .Compile();
                        var navigationExpression = Visit(includeExpression.NavigationExpression);

                        shaperExpressions.Add(Expression.Call(
                            includeMethod.MakeGenericMethod(includingClrType, relatedEntityClrType),
                            entityEntryVariable,
                            instanceVariable,
                            concreteEntityTypeVariable,
                            navigationExpression,
                            Expression.Constant(navigation),
                            Expression.Constant(inverseNavigation, typeof(INavigation)),
                            Expression.Constant(fixup)));

                        shaperExpressions.Add(instanceVariable);
                        shaperBlock = shaperBlock.Update(shaperBlock.Variables, shaperExpressions);

                        var jObjectExpressions = new List<Expression>(jObjectBlock.Expressions);
                        jObjectExpressions.RemoveAt(jObjectExpressions.Count - 1);

                        jObjectExpressions.Add(
                            jObjectCondition.Update(jObjectCondition.Test, jObjectCondition.IfTrue, shaperBlock));

                        return jObjectBlock.Update(jObjectBlock.Variables, jObjectExpressions);
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
                Action<TIncludingEntity, TIncludedEntity> fixup)
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
                Action<TIncludingEntity, TIncludedEntity> fixup)
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
                    entry.SetIsLoaded(navigation);
                    using (var enumerator = relatedEntities.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                        }
                    }
                }
            }

            private static void SetIsLoadedNoTracking(object entity, INavigation navigation)
                => ((ILazyLoader)((PropertyBase)navigation
                            .DeclaringEntityType
                            .GetServiceProperties()
                            .FirstOrDefault(p => p.ClrType == typeof(ILazyLoader)))
                        ?.Getter.GetClrValue(entity))
                    ?.SetLoaded(entity, navigation.Name);

            private static LambdaExpression GenerateFixup(
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

                return Expression.Lambda(Expression.Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
            }

            private static Expression AssignReferenceNavigation(
                ParameterExpression entity,
                ParameterExpression relatedEntity,
                INavigation navigation)
                => entity.MakeMemberAccess(navigation.GetMemberInfo(forConstruction: false, forSet: true))
                    .CreateAssignExpression(relatedEntity);

            private static Expression AddToCollectionNavigation(
                ParameterExpression entity,
                ParameterExpression relatedEntity,
                INavigation navigation)
                => Expression.Call(
                    Expression.Constant(navigation.GetCollectionAccessor()),
                    _collectionAccessorAddMethodInfo,
                    entity,
                    relatedEntity);

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

            private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
                => projectionBindingExpression.ProjectionMember != null
                    ? (int)((ConstantExpression)_selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : projectionBindingExpression.Index ?? throw new InvalidOperationException();

            private ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            {
                var index = GetProjectionIndex(projectionBindingExpression);
                return _selectExpression.Projection[index];
            }

            private static Expression CreateReadJTokenExpression(Expression jObjectExpression, string propertyName)
                => Expression.Call(jObjectExpression, _getItemMethodInfo, Expression.Constant(propertyName));

            private Expression CreateGetValueExpression(
                Expression jObjectExpression,
                IProperty property)
            {
                if (property.Name == StoreKeyConvention.JObjectPropertyName)
                {
                    return jObjectExpression;
                }

                var storeName = property.GetCosmosPropertyName();
                if (storeName.Length == 0)
                {
                    return Expression.Default(property.ClrType);
                }

                return CreateGetStoreValueExpression(jObjectExpression, storeName, property.ClrType, property.GetTypeMapping());
            }

            private Expression CreateGetStoreValueExpression(
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
                    innerExpression = CreateGetStoreValueExpression(
                        _jObjectParameter, rootReferenceExpression.Alias, typeof(JObject));
                }
                else if (jObjectExpression is ObjectAccessExpression objectAccessExpression)
                {
                    var innerAccessExpression = objectAccessExpression.AccessExpression;

                    innerExpression = CreateGetStoreValueExpression(
                        innerAccessExpression, ((IAccessExpression)innerAccessExpression).Name, typeof(JObject));
                }

                var jTokenExpression = Expression.Call(innerExpression, _getItemMethodInfo, Expression.Constant(storeName));
                Expression valueExpression;

                var converter = typeMapping?.Converter;
                if (converter != null)
                {
                    valueExpression = ConvertJTokenToType(jTokenExpression, converter.ProviderClrType);

                    valueExpression = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        valueExpression,
                        converter.ConvertFromProviderExpression.Body);

                    if (valueExpression.Type != clrType)
                    {
                        valueExpression = Expression.Convert(valueExpression, clrType);
                    }
                }
                else
                {
                    valueExpression = ConvertJTokenToType(jTokenExpression, clrType);
                }

                if (clrType.IsNullableType())
                {
                    valueExpression =
                        Expression.Condition(
                            Expression.Call(_isNullMethodInfo, jTokenExpression),
                            Expression.Default(valueExpression.Type),
                            valueExpression);
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
                => token == null ? default : token.ToObject<T>();

            private static bool IsNull(JToken token)
                => token == null || token.Type == JTokenType.Null;
        }

        private class QueryingEnumerable<T> : IEnumerable<T>
        {
            private readonly CosmosQueryContext _cosmosQueryContext;
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly SelectExpression _selectExpression;
            private readonly Func<QueryContext, JObject, T> _shaper;
            private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public QueryingEnumerable(
                CosmosQueryContext cosmosQueryContext,
                ISqlExpressionFactory sqlExpressionFactory,
                IQuerySqlGeneratorFactory querySqlGeneratorFactory,
                SelectExpression selectExpression,
                Func<QueryContext, JObject, T> shaper,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _cosmosQueryContext = cosmosQueryContext;
                _sqlExpressionFactory = sqlExpressionFactory;
                _querySqlGeneratorFactory = querySqlGeneratorFactory;
                _selectExpression = selectExpression;
                _shaper = shaper;
                _contextType = contextType;
                _logger = logger;
            }

            public IEnumerator<T> GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class Enumerator : IEnumerator<T>
            {
                private IEnumerator<JObject> _enumerator;
                private readonly CosmosQueryContext _cosmosQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, JObject, T> _shaper;
                private readonly ISqlExpressionFactory _sqlExpressionFactory;
                private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable)
                {
                    _cosmosQueryContext = queryingEnumerable._cosmosQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _sqlExpressionFactory = queryingEnumerable._sqlExpressionFactory;
                    _querySqlGeneratorFactory = queryingEnumerable._querySqlGeneratorFactory;
                    _contextType = queryingEnumerable._contextType;
                    _logger = queryingEnumerable._logger;
                }

                public T Current { get; private set; }

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    try
                    {
                        if (_enumerator == null)
                        {
                            var selectExpression = (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                                _sqlExpressionFactory, _cosmosQueryContext.ParameterValues).Visit(_selectExpression);

                            var sqlQuery = _querySqlGeneratorFactory.Create().GetSqlQuery(
                                selectExpression, _cosmosQueryContext.ParameterValues);

                            _enumerator = _cosmosQueryContext.CosmosClient
                                .ExecuteSqlQuery(
                                    _selectExpression.ContainerName,
                                    sqlQuery)
                                .GetEnumerator();
                        }

                        var hasNext = _enumerator.MoveNext();

                        Current
                            = hasNext
                                ? _shaper(_cosmosQueryContext, _enumerator.Current)
                                : default;

                        return hasNext;
                    }
                    catch (Exception exception)
                    {
                        _logger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public void Dispose()
                {
                    _enumerator?.Dispose();
                    _enumerator = null;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly CosmosQueryContext _cosmosQueryContext;
            private readonly SelectExpression _selectExpression;
            private readonly Func<QueryContext, JObject, T> _shaper;
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

            public AsyncQueryingEnumerable(
                CosmosQueryContext cosmosQueryContext,
                ISqlExpressionFactory sqlExpressionFactory,
                IQuerySqlGeneratorFactory querySqlGeneratorFactory,
                SelectExpression selectExpression,
                Func<QueryContext, JObject, T> shaper,
                Type contextType,
                IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            {
                _cosmosQueryContext = cosmosQueryContext;
                _sqlExpressionFactory = sqlExpressionFactory;
                _querySqlGeneratorFactory = querySqlGeneratorFactory;
                _selectExpression = selectExpression;
                _shaper = shaper;
                _contextType = contextType;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new AsyncEnumerator(this, cancellationToken);
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private IAsyncEnumerator<JObject> _enumerator;
                private readonly CosmosQueryContext _cosmosQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, JObject, T> _shaper;
                private readonly ISqlExpressionFactory _sqlExpressionFactory;
                private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
                private readonly Type _contextType;
                private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
                private readonly CancellationToken _cancellationToken;

                public AsyncEnumerator(AsyncQueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken)
                {
                    _cosmosQueryContext = queryingEnumerable._cosmosQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _sqlExpressionFactory = queryingEnumerable._sqlExpressionFactory;
                    _querySqlGeneratorFactory = queryingEnumerable._querySqlGeneratorFactory;
                    _contextType = queryingEnumerable._contextType;
                    _logger = queryingEnumerable._logger;
                    _cancellationToken = cancellationToken;
                }

                public T Current { get; private set; }

                public async ValueTask<bool> MoveNextAsync()
                {
                    try
                    {
                        if (_enumerator == null)
                        {
                            var selectExpression = (SelectExpression)new InExpressionValuesExpandingExpressionVisitor(
                               _sqlExpressionFactory, _cosmosQueryContext.ParameterValues).Visit(_selectExpression);

                            _enumerator = _cosmosQueryContext.CosmosClient
                                .ExecuteSqlQueryAsync(
                                    _selectExpression.ContainerName,
                                    _querySqlGeneratorFactory.Create().GetSqlQuery(selectExpression, _cosmosQueryContext.ParameterValues))
                                .GetAsyncEnumerator(_cancellationToken);

                        }

                        var hasNext = await _enumerator.MoveNextAsync();

                        Current
                            = hasNext
                                ? _shaper(_cosmosQueryContext, _enumerator.Current)
                                : default;

                        return hasNext;
                    }
                    catch (Exception exception)
                    {
                        _logger.QueryIterationFailed(_contextType, exception);

                        throw;
                    }
                }

                public ValueTask DisposeAsync()
                {
                    _enumerator?.DisposeAsync();
                    _enumerator = null;

                    return default;
                }
            }
        }

        private class InExpressionValuesExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly IReadOnlyDictionary<string, object> _parametersValues;

            public InExpressionValuesExpandingExpressionVisitor(
                ISqlExpressionFactory sqlExpressionFactory, IReadOnlyDictionary<string, object> parametersValues)
            {
                _sqlExpressionFactory = sqlExpressionFactory;
                _parametersValues = parametersValues;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is InExpression inExpression)
                {
                    var inValues = new List<object>();
                    var hasNullValue = false;
                    CoreTypeMapping typeMapping = null;

                    switch (inExpression.Values)
                    {
                        case SqlConstantExpression sqlConstant:
                            {
                                typeMapping = sqlConstant.TypeMapping;
                                var values = (IEnumerable)sqlConstant.Value;
                                foreach (var value in values)
                                {
                                    if (value == null)
                                    {
                                        hasNullValue = true;
                                        continue;
                                    }

                                    inValues.Add(value);
                                }
                            }
                            break;

                        case SqlParameterExpression sqlParameter:
                            {
                                typeMapping = sqlParameter.TypeMapping;
                                var values = (IEnumerable)_parametersValues[sqlParameter.Name];
                                foreach (var value in values)
                                {
                                    if (value == null)
                                    {
                                        hasNullValue = true;
                                        continue;
                                    }

                                    inValues.Add(value);
                                }
                            }
                            break;
                    }

                    var updatedInExpression = inValues.Count > 0
                        ? _sqlExpressionFactory.In(
                            (SqlExpression)Visit(inExpression.Item),
                            _sqlExpressionFactory.Constant(inValues, typeMapping),
                            inExpression.Negated)
                        : null;

                    var nullCheckExpression = hasNullValue
                        ? _sqlExpressionFactory.IsNull(inExpression.Item)
                        : null;

                    if (updatedInExpression != null && nullCheckExpression != null)
                    {
                        return _sqlExpressionFactory.OrElse(updatedInExpression, nullCheckExpression);
                    }

                    if (updatedInExpression == null && nullCheckExpression == null)
                    {
                        return _sqlExpressionFactory.Equal(_sqlExpressionFactory.Constant(true), _sqlExpressionFactory.Constant(false));
                    }

                    return (SqlExpression)updatedInExpression ?? nullCheckExpression;
                }

                return base.Visit(expression);
            }
        }
    }
}
