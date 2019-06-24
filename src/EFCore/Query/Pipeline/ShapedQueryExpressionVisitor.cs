// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public abstract class ShapedQueryCompilingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _singleMethodInfo
            = typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(nameof(Enumerable.Single))
                .Single(mi => mi.GetParameters().Length == 1);

        private static readonly MethodInfo _singleOrDefaultMethodInfo
            = typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(nameof(Enumerable.SingleOrDefault))
                .Single(mi => mi.GetParameters().Length == 1);

        private static readonly PropertyInfo _cancellationTokenMemberInfo
            = typeof(QueryContext).GetProperty(nameof(QueryContext.CancellationToken));

        private readonly IEntityMaterializerSource _entityMaterializerSource;
        protected readonly bool TrackQueryResults;
        private readonly Expression _cancellationTokenParameter;
        protected readonly bool Async;

        public ShapedQueryCompilingExpressionVisitor(IEntityMaterializerSource entityMaterializerSource, bool trackQueryResults, bool async)
        {
            _entityMaterializerSource = entityMaterializerSource;
            TrackQueryResults = trackQueryResults;
            Async = async;
            if (async)
            {
                _cancellationTokenParameter = Expression.MakeMemberAccess(
                    QueryCompilationContext.QueryContextParameter,
                    _cancellationTokenMemberInfo);
            }
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    var serverEnumerable = VisitShapedQueryExpression(shapedQueryExpression);
                    switch (shapedQueryExpression.ResultType)
                    {
                        case ResultType.Enumerable:
                            return serverEnumerable;

                        case ResultType.Single:
                            return Async
                                ? Expression.Call(
                                    _singleAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.TryGetSequenceType()),
                                    serverEnumerable,
                                    _cancellationTokenParameter)
                                : Expression.Call(
                                    _singleMethodInfo.MakeGenericMethod(serverEnumerable.Type.TryGetSequenceType()),
                                    serverEnumerable);

                        case ResultType.SingleWithDefault:
                            return Async
                                ? Expression.Call(
                                    _singleOrDefaultAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.TryGetSequenceType()),
                                    serverEnumerable,
                                    _cancellationTokenParameter)
                                : Expression.Call(
                                    _singleOrDefaultMethodInfo.MakeGenericMethod(serverEnumerable.Type.TryGetSequenceType()),
                                    serverEnumerable);
                    }

                    break;
            }

            return base.VisitExtension(extensionExpression);
        }

        private static readonly MethodInfo _singleAsyncMethodInfo
            = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethods(nameof(ShapedQueryCompilingExpressionVisitor.SingleAsync))
                .Single(mi => mi.GetParameters().Length == 2);

        private static readonly MethodInfo _singleOrDefaultAsyncMethodInfo
            = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethods(nameof(ShapedQueryCompilingExpressionVisitor.SingleOrDefaultAsync))
                .Single(mi => mi.GetParameters().Length == 2);

        private async static Task<TSource> SingleAsync<TSource>(
            IAsyncEnumerable<TSource> asyncEnumerable,
            CancellationToken cancellationToken = default)
        {
            await using (var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken))
            {
                if (!(await enumerator.MoveNextAsync()))
                {
                    throw new InvalidOperationException();
                }

                var result = enumerator.Current;

                if (await enumerator.MoveNextAsync())
                {
                    throw new InvalidOperationException();
                }
                return result;
            }
        }

        private async static Task<TSource> SingleOrDefaultAsync<TSource>(
            IAsyncEnumerable<TSource> asyncEnumerable,
            CancellationToken cancellationToken = default)
        {
            await using (var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken))
            {
                if (!(await enumerator.MoveNextAsync()))
                {
                    return default;
                }

                var result = enumerator.Current;

                if (await enumerator.MoveNextAsync())
                {
                    throw new InvalidOperationException();
                }
                return result;
            }
        }

        protected abstract Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression);

        protected virtual Expression InjectEntityMaterializer(Expression expression)
        {
            return new EntityMaterializerInjectingExpressionVisitor(
                _entityMaterializerSource, TrackQueryResults).Inject(expression);
        }

        private class EntityMaterializerInjectingExpressionVisitor : ExpressionVisitor
        {
            private static readonly ConstructorInfo _materializationContextConstructor
                = typeof(MaterializationContext).GetConstructors().Single(ci => ci.GetParameters().Length == 2);

            private static readonly PropertyInfo _dbContextMemberInfo
                = typeof(QueryContext).GetProperty(nameof(QueryContext.Context));
            private static readonly PropertyInfo _stateManagerMemberInfo
                = typeof(QueryContext).GetProperty(nameof(QueryContext.StateManager));
            private static readonly PropertyInfo _entityMemberInfo
                = typeof(InternalEntityEntry).GetProperty(nameof(InternalEntityEntry.Entity));

            private static readonly MethodInfo _tryGetEntryMethodInfo
                = typeof(IStateManager).GetTypeInfo().GetDeclaredMethods(nameof(IStateManager.TryGetEntry))
                    .Single(mi => mi.GetParameters().Length == 4);
            private static readonly MethodInfo _startTrackingMethodInfo
                = typeof(QueryContext).GetMethod(nameof(QueryContext.StartTracking), new[] { typeof(IEntityType), typeof(object), typeof(ValueBuffer) });

            private readonly IEntityMaterializerSource _entityMaterializerSource;
            private readonly bool _trackQueryResults;
            private int _currentEntityIndex;

            public EntityMaterializerInjectingExpressionVisitor(
                IEntityMaterializerSource entityMaterializerSource, bool trackQueryResults)
            {
                _entityMaterializerSource = entityMaterializerSource;
                _trackQueryResults = trackQueryResults;
            }

            public Expression Inject(Expression expression)
            {
                return Visit(expression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is EntityShaperExpression entityShaperExpression)
                {
                    return ProcessEntityShaper(entityShaperExpression);
                }

                // TODO: Remove when InMemory implements client eval projection
                if (extensionExpression is EntityValuesExpression entityValuesExpression)
                {
                    return Expression.NewArrayInit(
                        typeof(object),
                        entityValuesExpression.EntityType.GetProperties()
                            .Select(p => _entityMaterializerSource.CreateReadValueExpression(
                                entityValuesExpression.ValueBufferExpression,
                                typeof(object),
                                p.GetIndex(),
                                p)));
                }

                if (extensionExpression is ProjectionBindingExpression)
                {
                    return extensionExpression;
                }

                return base.VisitExtension(extensionExpression);
            }

            private Expression ProcessEntityShaper(EntityShaperExpression entityShaperExpression)
            {
                _currentEntityIndex++;
                var expressions = new List<Expression>();
                var variables = new List<ParameterExpression>();

                var entityType = entityShaperExpression.EntityType;
                var valueBuffer = entityShaperExpression.ValueBufferExpression;

                var primaryKey = entityType.FindPrimaryKey();

                if (_trackQueryResults && primaryKey == null)
                {
                    throw new InvalidOperationException("A tracking query contains entityType without key in final result.");
                }

                if (_trackQueryResults)
                {
                    var entry = Expression.Variable(typeof(InternalEntityEntry), "entry" + _currentEntityIndex);
                    var hasNullKey = Expression.Variable(typeof(bool), "hasNullKey" + _currentEntityIndex);
                    variables.Add(entry);
                    variables.Add(hasNullKey);

                    expressions.Add(
                        Expression.Assign(
                            entry,
                            Expression.Call(
                                Expression.MakeMemberAccess(
                                    QueryCompilationContext.QueryContextParameter,
                                    _stateManagerMemberInfo),
                                _tryGetEntryMethodInfo,
                                Expression.Constant(primaryKey),
                                Expression.NewArrayInit(
                                    typeof(object),
                                    primaryKey.Properties
                                        .Select(p => _entityMaterializerSource.CreateReadValueExpression(
                                            valueBuffer,
                                            typeof(object),
                                            p.GetIndex(),
                                            p))),
                                Expression.Constant(!entityShaperExpression.Nullable),
                                hasNullKey)));

                    expressions.Add(Expression.Condition(
                        hasNullKey,
                        Expression.Constant(null, entityType.ClrType),
                        Expression.Condition(
                            Expression.NotEqual(
                                entry,
                                Expression.Constant(default(InternalEntityEntry), typeof(InternalEntityEntry))),
                            Expression.Convert(
                                Expression.MakeMemberAccess(entry, _entityMemberInfo),
                                entityType.ClrType),
                            MaterializeEntity(entityShaperExpression))));
                }
                else
                {
                    expressions.Add(Expression.Condition(
                        (primaryKey != null
                            ? primaryKey.Properties.Select(p =>
                                    Expression.Equal(
                                        _entityMaterializerSource.CreateReadValueExpression(
                                            entityShaperExpression.ValueBufferExpression,
                                            typeof(object),
                                            p.GetIndex(),
                                            p),
                                        Expression.Constant(null)))
                                    .Aggregate((a, b) => Expression.OrElse(a, b))
                            : entityType.GetProperties()
                                .Select(p =>
                                        Expression.Equal(
                                            _entityMaterializerSource.CreateReadValueExpression(
                                                entityShaperExpression.ValueBufferExpression,
                                                typeof(object),
                                                p.GetIndex(),
                                                p),
                                            Expression.Constant(null)))
                                        .Aggregate((a, b) => Expression.AndAlso(a, b))),
                            Expression.Constant(null, entityType.ClrType),
                            MaterializeEntity(entityShaperExpression)));
                }

                return Expression.Block(variables, expressions);
            }

            private Expression MaterializeEntity(EntityShaperExpression entityShaperExpression)
            {
                var entityType = entityShaperExpression.EntityType;
                var valueBuffer = entityShaperExpression.ValueBufferExpression;
                var expressions = new List<Expression>();
                var variables = new List<ParameterExpression>();
                var returnType = entityType.ClrType;
                var valueBufferConstructor
                    = typeof(ValueBuffer).GetTypeInfo().DeclaredConstructors.Single(ci => ci.GetParameters().Length == 1);

                var materializationContext = Expression.Variable(
                    typeof(MaterializationContext), "materializationContext" + _currentEntityIndex);

                variables.Add(materializationContext);
                expressions.Add(
                    Expression.Assign(
                        materializationContext,
                        Expression.New(
                            _materializationContextConstructor,
                            valueBuffer,
                            Expression.MakeMemberAccess(
                                QueryCompilationContext.QueryContextParameter,
                                _dbContextMemberInfo))));

                var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();

                Expression materializationExpression = null;
                Expression shadowValuesExpression = Expression.Constant(ValueBuffer.Empty);
                if (concreteEntityTypes.Count == 1)
                {
                    materializationExpression
                        = _entityMaterializerSource.CreateMaterializeExpression(
                            concreteEntityTypes[0],
                            "instance" + _currentEntityIndex,
                            materializationContext);

                    if (_trackQueryResults)
                    {
                        var shadowProperties = concreteEntityTypes[0].GetProperties().Where(p => p.IsShadowProperty());
                        if (shadowProperties.Any())
                        {
                            shadowValuesExpression = Expression.New(
                                valueBufferConstructor,
                                Expression.NewArrayInit(
                                    typeof(object),
                                    shadowProperties.Select(p => _entityMaterializerSource.CreateReadValueExpression(
                                        valueBuffer,
                                        typeof(object),
                                        p.GetIndex(),
                                        p))));
                        }
                    }
                }
                else
                {
                    var firstEntityType = concreteEntityTypes[0];
                    var discriminatorProperty = firstEntityType.GetDiscriminatorProperty();
                    var discriminatorValueVariable = Expression.Variable(discriminatorProperty.ClrType);
                    variables.Add(discriminatorValueVariable);

                    expressions.Add(
                        Expression.Assign(
                            discriminatorValueVariable,
                            _entityMaterializerSource.CreateReadValueExpression(
                                valueBuffer,
                                discriminatorProperty.ClrType,
                                discriminatorProperty.GetIndex(),
                                discriminatorProperty)));

                    materializationExpression = Expression.Block(
                        Expression.Throw(
                            Expression.Call(
                                _createUnableToDiscriminateException,
                                Expression.Constant(entityType),
                                Expression.Convert(discriminatorValueVariable, typeof(object)))),
                        Expression.Constant(null, returnType));

                    foreach (var concreteEntityType in concreteEntityTypes)
                    {
                        var discriminatorValue
                            = Expression.Constant(
                                concreteEntityType.GetDiscriminatorValue(),
                                discriminatorProperty.ClrType);

                        var materializer = _entityMaterializerSource
                            .CreateMaterializeExpression(concreteEntityType, "instance", materializationContext);

                        materializationExpression = Expression.Condition(
                            Expression.Equal(discriminatorValueVariable, discriminatorValue),
                            Expression.Convert(materializer, returnType),
                            materializationExpression);

                        if (_trackQueryResults)
                        {
                            var shadowProperties = concreteEntityType.GetProperties().Where(p => p.IsShadowProperty());
                            var shadowValues = shadowProperties.Any()
                                ? Expression.New(
                                    valueBufferConstructor,
                                    Expression.NewArrayInit(
                                        typeof(object),
                                        shadowProperties.Select(p => _entityMaterializerSource.CreateReadValueExpression(
                                            valueBuffer,
                                            typeof(object),
                                            p.GetIndex(),
                                            p))))
                                : (Expression)Expression.Constant(ValueBuffer.Empty);

                            shadowValuesExpression = Expression.Condition(
                                    Expression.Equal(discriminatorValueVariable, discriminatorValue),
                                    shadowValues,
                                    shadowValuesExpression);

                        }
                    }
                }

                Expression result;
                if (materializationExpression is BlockExpression blockExpression)
                {
                    expressions.AddRange(blockExpression.Expressions.Take(blockExpression.Expressions.Count - 1));
                    variables.AddRange(blockExpression.Variables);
                    result = blockExpression.Expressions.Last();
                }
                else
                {
                    // Possible expressions are
                    // NewExpression when only ctor initialization
                    // MethodCallExpression when using factory method to create entityType
                    var instanceVariable = Expression.Variable(returnType, "instance" + _currentEntityIndex);
                    variables.Add(instanceVariable);
                    expressions.Add(Expression.Assign(instanceVariable, materializationExpression));
                    result = instanceVariable;
                }

                if (_trackQueryResults)
                {
                    expressions.Add(
                        Expression.Call(
                            QueryCompilationContext.QueryContextParameter,
                            _startTrackingMethodInfo,
                            Expression.Constant(entityType),
                            result,
                            shadowValuesExpression));
                }

                expressions.Add(result);

                return Expression.Block(
                    returnType,
                    variables,
                    expressions);
            }

            private static readonly MethodInfo _createUnableToDiscriminateException
                = typeof(EntityMaterializerInjectingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

            [UsedImplicitly]
            private static Exception CreateUnableToDiscriminateException(IEntityType entityType, object discriminator)
                => new InvalidOperationException(CoreStrings.UnableToDiscriminate(entityType.DisplayName(), discriminator?.ToString()));
        }
    }
}
