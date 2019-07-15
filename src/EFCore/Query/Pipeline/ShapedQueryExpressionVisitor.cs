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
        private readonly Expression _cancellationTokenParameter;
        private readonly EntityMaterializerInjectingExpressionVisitor _entityMaterializerInjectingExpressionVisitor;

        protected ShapedQueryCompilingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            IEntityMaterializerSource entityMaterializerSource)
        {
            _entityMaterializerSource = entityMaterializerSource;
            TrackQueryResults = queryCompilationContext.TrackQueryResults;
            _entityMaterializerInjectingExpressionVisitor =
                new EntityMaterializerInjectingExpressionVisitor(entityMaterializerSource, TrackQueryResults);
            Async = queryCompilationContext.Async;
            if (Async)
            {
                _cancellationTokenParameter = Expression.MakeMemberAccess(
                    QueryCompilationContext.QueryContextParameter,
                    _cancellationTokenMemberInfo);
            }
        }


        protected bool TrackQueryResults { get; }

        protected bool Async { get; }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
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

        private static async Task<TSource> SingleAsync<TSource>(
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

        private static async Task<TSource> SingleOrDefaultAsync<TSource>(
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

        protected virtual Expression CreateReadValueExpression(
            Expression valueBufferExpression,
            Type type,
            int index,
            IPropertyBase property)
            => _entityMaterializerSource.CreateReadValueExpression(valueBufferExpression, type, index, property);

        protected virtual Expression InjectEntityMaterializers(Expression expression)
            => _entityMaterializerInjectingExpressionVisitor.Inject(expression);

        private class EntityMaterializerInjectingExpressionVisitor : ExpressionVisitor
        {
            private static readonly ConstructorInfo _materializationContextConstructor
                = typeof(MaterializationContext).GetConstructors().Single(ci => ci.GetParameters().Length == 2);
            private static readonly ConstructorInfo _valueBufferConstructor
                = typeof(ValueBuffer).GetTypeInfo().DeclaredConstructors.Single(ci => ci.GetParameters().Length == 1);

            private static readonly PropertyInfo _dbContextMemberInfo
                = typeof(QueryContext).GetProperty(nameof(QueryContext.Context));
            private static readonly PropertyInfo _stateManagerMemberInfo
                = typeof(QueryContext).GetProperty(nameof(QueryContext.StateManager));
            private static readonly PropertyInfo _entityMemberInfo
                = typeof(InternalEntityEntry).GetProperty(nameof(InternalEntityEntry.Entity));
            private static readonly PropertyInfo _entityTypeMemberInfo
                = typeof(InternalEntityEntry).GetProperty(nameof(InternalEntityEntry.EntityType));

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

            public Expression Inject(Expression expression) => Visit(expression);

            protected override Expression VisitExtension(Expression extensionExpression)
                => extensionExpression is EntityShaperExpression entityShaperExpression
                    ? ProcessEntityShaper(entityShaperExpression)
                    : base.VisitExtension(extensionExpression);

            private Expression ProcessEntityShaper(EntityShaperExpression entityShaperExpression)
            {
                _currentEntityIndex++;

                var expressions = new List<Expression>();
                var variables = new List<ParameterExpression>();

                var entityType = entityShaperExpression.EntityType;

                var materializationContextVariable = Expression.Variable(typeof(MaterializationContext),
                    "materializationContext" + _currentEntityIndex);
                variables.Add(materializationContextVariable);
                expressions.Add(
                    Expression.Assign(
                        materializationContextVariable,
                        Expression.New(
                            _materializationContextConstructor,
                            entityShaperExpression.ValueBufferExpression,
                            Expression.MakeMemberAccess(
                                QueryCompilationContext.QueryContextParameter,
                                _dbContextMemberInfo))));

                var valueBufferExpression = Expression.Call(materializationContextVariable, MaterializationContext.GetValueBufferMethod);

                var primaryKey = entityType.FindPrimaryKey();

                if (_trackQueryResults && primaryKey == null)
                {
                    throw new InvalidOperationException("A tracking query contains entityType without key in final result.");
                }

                var concreteEntityTypeVariable = Expression.Variable(typeof(IEntityType),
                    "entityType" + _currentEntityIndex);
                variables.Add(concreteEntityTypeVariable);

                var instanceVariable = Expression.Variable(entityType.ClrType, "instance" + _currentEntityIndex);
                variables.Add(instanceVariable);
                expressions.Add(Expression.Assign(
                                    instanceVariable,
                                    Expression.Constant(null, entityType.ClrType)));

                if (_trackQueryResults)
                {
                    var entryVariable = Expression.Variable(typeof(InternalEntityEntry), "entry" + _currentEntityIndex);
                    var hasNullKeyVariable = Expression.Variable(typeof(bool), "hasNullKey" + _currentEntityIndex);
                    variables.Add(entryVariable);
                    variables.Add(hasNullKeyVariable);

                    expressions.Add(
                        Expression.Assign(
                            entryVariable,
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
                                            valueBufferExpression,
                                            typeof(object),
                                            p.GetIndex(),
                                            p))),
                                Expression.Constant(!entityShaperExpression.Nullable),
                                hasNullKeyVariable)));

                    expressions.Add(Expression.IfThen(
                        Expression.Not(hasNullKeyVariable),
                        Expression.IfThenElse(
                            Expression.NotEqual(
                                entryVariable,
                                Expression.Constant(default(InternalEntityEntry), typeof(InternalEntityEntry))),
                            Expression.Block(
                                Expression.Assign(
                                    concreteEntityTypeVariable,
                                    Expression.MakeMemberAccess(entryVariable, _entityTypeMemberInfo)),
                                Expression.Assign(instanceVariable, Expression.Convert(
                                    Expression.MakeMemberAccess(entryVariable, _entityMemberInfo),
                                    entityType.ClrType))),
                            MaterializeEntity(
                                entityType, materializationContextVariable, concreteEntityTypeVariable, instanceVariable, entryVariable))));
                }
                else
                {
                    expressions.Add(Expression.IfThen(
                        primaryKey != null
                            ? primaryKey.Properties.Select(p =>
                                    Expression.NotEqual(
                                        _entityMaterializerSource.CreateReadValueExpression(
                                            valueBufferExpression,
                                            typeof(object),
                                            p.GetIndex(),
                                            p),
                                        Expression.Constant(null)))
                                    .Aggregate((a, b) => Expression.AndAlso(a, b))
                            : entityType.GetProperties()
                                .Select(p =>
                                        Expression.NotEqual(
                                            _entityMaterializerSource.CreateReadValueExpression(
                                                valueBufferExpression,
                                                typeof(object),
                                                p.GetIndex(),
                                                p),
                                            Expression.Constant(null)))
                                        .Aggregate((a, b) => Expression.OrElse(a, b)),
                            MaterializeEntity(
                                entityType, materializationContextVariable, concreteEntityTypeVariable, instanceVariable, null)));
                }

                expressions.Add(instanceVariable);
                return Expression.Block(variables, expressions);
            }

            private Expression MaterializeEntity(
                IEntityType entityType,
                ParameterExpression materializationContextVariable,
                ParameterExpression concreteEntityTypeVariable,
                ParameterExpression instanceVariable,
                ParameterExpression entryVariable)
            {
                var expressions = new List<Expression>();
                var variables = new List<ParameterExpression>();

                var shadowValuesVariable = Expression.Variable(typeof(ValueBuffer),
                    "shadowValueBuffer" + _currentEntityIndex);
                variables.Add(shadowValuesVariable);
                expressions.Add(
                    Expression.Assign(
                        shadowValuesVariable,
                        Expression.Constant(ValueBuffer.Empty)));

                var returnType = entityType.ClrType;
                Expression materializationExpression;
                var valueBufferExpression = Expression.Call(materializationContextVariable, MaterializationContext.GetValueBufferMethod);
                var expressionContext = (returnType, materializationContextVariable, concreteEntityTypeVariable, shadowValuesVariable);
                var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
                var firstEntityType = concreteEntityTypes[0];
                if (concreteEntityTypes.Count == 1)
                {
                    materializationExpression = CreateFullMaterializeExpression(firstEntityType, expressionContext);
                }
                else
                {
                    var discriminatorProperty = firstEntityType.GetDiscriminatorProperty();
                    var discriminatorValueVariable = Expression.Variable(discriminatorProperty.ClrType, "discriminator" + _currentEntityIndex);
                    variables.Add(discriminatorValueVariable);

                    expressions.Add(
                        Expression.Assign(
                            discriminatorValueVariable,
                            _entityMaterializerSource.CreateReadValueExpression(
                                valueBufferExpression,
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

                        materializationExpression = Expression.Condition(
                            Expression.Equal(discriminatorValueVariable, discriminatorValue),
                            CreateFullMaterializeExpression(concreteEntityType, expressionContext),
                            materializationExpression);
                    }
                }

                expressions.Add(Expression.Assign(instanceVariable, materializationExpression));

                if (_trackQueryResults)
                {
                    expressions.Add(
                        Expression.Assign(
                            entryVariable, Expression.Call(
                                QueryCompilationContext.QueryContextParameter,
                                _startTrackingMethodInfo,
                                concreteEntityTypeVariable,
                                instanceVariable,
                                shadowValuesVariable)));
                }

                expressions.Add(instanceVariable);

                return Expression.Block(
                    returnType,
                    variables,
                    expressions);
            }

            private BlockExpression CreateFullMaterializeExpression(
                IEntityType concreteEntityType,
                in (Type ReturnType,
                ParameterExpression MaterializationContextVariable,
                ParameterExpression ConcreteEntityTypeVariable,
                ParameterExpression ShadowValuesVariable) materializeExpressionContext)
            {
                var (returnType,
                    materializationContextVariable,
                    concreteEntityTypeVariable,
                    shadowValuesVariable) = materializeExpressionContext;

                var blockExpressions = new List<Expression>(3)
                        {
                            Expression.Assign(
                                concreteEntityTypeVariable,
                                Expression.Constant(concreteEntityType))
                        };

                var materializer = _entityMaterializerSource
                    .CreateMaterializeExpression(concreteEntityType, "instance", materializationContextVariable);

                if (_trackQueryResults
                    && concreteEntityType.ShadowPropertyCount() > 0)
                {
                    var valueBufferExpression = Expression.Call(materializationContextVariable, MaterializationContext.GetValueBufferMethod);
                    var shadowProperties = concreteEntityType.GetProperties().Where(p => p.IsShadowProperty());
                    blockExpressions.Add(
                        Expression.Assign(
                            shadowValuesVariable,
                            Expression.New(
                                _valueBufferConstructor,
                                Expression.NewArrayInit(
                                    typeof(object),
                                    shadowProperties.Select(p => _entityMaterializerSource.CreateReadValueExpression(
                                        valueBufferExpression,
                                        typeof(object),
                                        p.GetIndex(),
                                        p))))));
                }

                materializer = materializer.Type == returnType
                    ? materializer
                    : Expression.Convert(materializer, returnType);
                blockExpressions.Add(materializer);

                return Expression.Block(blockExpressions);
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
