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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ShapedQueryCompilingExpressionVisitor : ExpressionVisitor
    {
        private static readonly PropertyInfo _cancellationTokenMemberInfo
            = typeof(QueryContext).GetProperty(nameof(QueryContext.CancellationToken));

        private readonly Expression _cancellationTokenParameter;
        private readonly EntityMaterializerInjectingExpressionVisitor _entityMaterializerInjectingExpressionVisitor;
        private readonly ConstantVerifyingExpressionVisitor _constantVerifyingExpressionVisitor;

        protected ShapedQueryCompilingExpressionVisitor(
            ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext)
        {
            Dependencies = dependencies;
            IsTracking = queryCompilationContext.IsTracking;

            _entityMaterializerInjectingExpressionVisitor =
                new EntityMaterializerInjectingExpressionVisitor(
                    dependencies.EntityMaterializerSource,
                    queryCompilationContext.IsTracking);

            _constantVerifyingExpressionVisitor = new ConstantVerifyingExpressionVisitor(dependencies.TypeMappingSource);

            IsBuffering = queryCompilationContext.IsBuffering;
            IsAsync = queryCompilationContext.IsAsync;

            if (queryCompilationContext.IsAsync)
            {
                _cancellationTokenParameter = Expression.MakeMemberAccess(
                    QueryCompilationContext.QueryContextParameter,
                    _cancellationTokenMemberInfo);
            }
        }

        protected virtual ShapedQueryCompilingExpressionVisitorDependencies Dependencies { get; }

        protected virtual bool IsTracking { get; }

        public virtual bool IsBuffering { get; internal set; }

        protected virtual bool IsAsync { get; }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
                var serverEnumerable = VisitShapedQueryExpression(shapedQueryExpression);
                switch (shapedQueryExpression.ResultCardinality)
                {
                    case ResultCardinality.Enumerable:
                        return serverEnumerable;

                    case ResultCardinality.Single:
                        return IsAsync
                            ? Expression.Call(
                                _singleAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.TryGetSequenceType()),
                                serverEnumerable,
                                _cancellationTokenParameter)
                            : Expression.Call(
                                EnumerableMethods.SingleWithoutPredicate.MakeGenericMethod(serverEnumerable.Type.TryGetSequenceType()),
                                serverEnumerable);

                    case ResultCardinality.SingleOrDefault:
                        return IsAsync
                            ? Expression.Call(
                                _singleOrDefaultAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.TryGetSequenceType()),
                                serverEnumerable,
                                _cancellationTokenParameter)
                            : Expression.Call(
                                EnumerableMethods.SingleOrDefaultWithoutPredicate.MakeGenericMethod(
                                    serverEnumerable.Type.TryGetSequenceType()),
                                serverEnumerable);
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        private static readonly MethodInfo _singleAsyncMethodInfo
            = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethods(nameof(SingleAsync))
                .Single(mi => mi.GetParameters().Length == 2);

        private static readonly MethodInfo _singleOrDefaultAsyncMethodInfo
            = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethods(nameof(SingleOrDefaultAsync))
                .Single(mi => mi.GetParameters().Length == 2);

        private static async Task<TSource> SingleAsync<TSource>(
            IAsyncEnumerable<TSource> asyncEnumerable,
            CancellationToken cancellationToken = default)
        {
            await using (var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken))
            {
                if (!await enumerator.MoveNextAsync())
                {
                    throw new InvalidOperationException("Enumerator failed to MoveNextAsync.");
                }

                var result = enumerator.Current;

                if (await enumerator.MoveNextAsync())
                {
                    throw new InvalidOperationException("Enumerator failed to MoveNextAsync.");
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
                    throw new InvalidOperationException("Enumerator failed to MoveNextAsync.");
                }

                return result;
            }
        }

        protected abstract Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression);

        protected virtual Expression InjectEntityMaterializers(Expression expression)
        {
            _constantVerifyingExpressionVisitor.Visit(expression);

            return _entityMaterializerInjectingExpressionVisitor.Inject(expression);
        }

        private class ConstantVerifyingExpressionVisitor : ExpressionVisitor
        {
            private readonly ITypeMappingSource _typeMappingSource;

            public ConstantVerifyingExpressionVisitor(ITypeMappingSource typeMappingSource)
            {
                _typeMappingSource = typeMappingSource;
            }

            private bool ValidConstant(ConstantExpression constantExpression)
            {
                return constantExpression.Value == null
                    || _typeMappingSource.FindMapping(constantExpression.Type) != null;
            }

            protected override Expression VisitConstant(ConstantExpression constantExpression)
            {
                if (!ValidConstant(constantExpression))
                {
                    throw new InvalidOperationException(
                        CoreStrings.ClientProjectionCapturingConstantInTree(constantExpression.Type.DisplayName()));
                }

                return constantExpression;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (RemoveConvert(methodCallExpression.Object) is ConstantExpression constantInstance
                    && !ValidConstant(constantInstance))
                {
                    throw new InvalidOperationException(
                        CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                            constantInstance.Type.DisplayName(),
                            methodCallExpression.Method.Name));
                }

                foreach (var argument in methodCallExpression.Arguments)
                {
                    if (RemoveConvert(argument) is ConstantExpression constantArgument
                        && !ValidConstant(constantArgument))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                                constantArgument.Type.DisplayName(),
                                methodCallExpression.Method.Name));
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                return extensionExpression is EntityShaperExpression
                    || extensionExpression is ProjectionBindingExpression
                        ? extensionExpression
                        : base.VisitExtension(extensionExpression);
            }

            private static Expression RemoveConvert(Expression expression)
            {
                while (expression != null
                    && (expression.NodeType == ExpressionType.Convert
                        || expression.NodeType == ExpressionType.ConvertChecked))
                {
                    expression = RemoveConvert(((UnaryExpression)expression).Operand);
                }

                return expression;
            }
        }

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
                = typeof(QueryContext).GetMethod(
                    nameof(QueryContext.StartTracking), new[] { typeof(IEntityType), typeof(object), typeof(ValueBuffer) });

            private readonly IEntityMaterializerSource _entityMaterializerSource;
            private readonly bool _trackQueryResults;
            private readonly ISet<IEntityType> _visitedEntityTypes = new HashSet<IEntityType>();
            private int _currentEntityIndex;

            public EntityMaterializerInjectingExpressionVisitor(
                IEntityMaterializerSource entityMaterializerSource, bool trackQueryResults)
            {
                _entityMaterializerSource = entityMaterializerSource;
                _trackQueryResults = trackQueryResults;
            }

            public Expression Inject(Expression expression)
            {
                var result = Visit(expression);
                if (_trackQueryResults)
                {
                    foreach (var entityType in _visitedEntityTypes)
                    {
                        if (entityType.FindOwnership() is IForeignKey ownership
                            && !ContainsOwner(ownership.PrincipalEntityType))
                        {
                            throw new InvalidOperationException(
                                "A tracking query projects owned entity without corresponding owner in result. "
                                + "Owned entities cannot be tracked without their owner. "
                                + "Either include the owner entity in the result or make query non-tracking using AsNoTracking().");
                        }
                    }

                    bool ContainsOwner(IEntityType owner)
                        => owner != null && (_visitedEntityTypes.Contains(owner) || ContainsOwner(owner.BaseType));
                }

                return result;
            }

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

                var materializationContextVariable = Expression.Variable(
                    typeof(MaterializationContext),
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

                var concreteEntityTypeVariable = Expression.Variable(
                    typeof(IEntityType),
                    "entityType" + _currentEntityIndex);
                variables.Add(concreteEntityTypeVariable);

                var instanceVariable = Expression.Variable(entityType.ClrType, "instance" + _currentEntityIndex);
                variables.Add(instanceVariable);
                expressions.Add(
                    Expression.Assign(
                        instanceVariable,
                        Expression.Constant(null, entityType.ClrType)));

                if (_trackQueryResults
                    && primaryKey != null)
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
                                        .Select(
                                            p => _entityMaterializerSource.CreateReadValueExpression(
                                                valueBufferExpression,
                                                typeof(object),
                                                p.GetIndex(),
                                                p))),
                                Expression.Constant(!entityShaperExpression.IsNullable),
                                hasNullKeyVariable)));

                    expressions.Add(
                        Expression.IfThen(
                            Expression.Not(hasNullKeyVariable),
                            Expression.IfThenElse(
                                Expression.NotEqual(
                                    entryVariable,
                                    Expression.Constant(default(InternalEntityEntry), typeof(InternalEntityEntry))),
                                Expression.Block(
                                    Expression.Assign(
                                        concreteEntityTypeVariable,
                                        Expression.MakeMemberAccess(entryVariable, _entityTypeMemberInfo)),
                                    Expression.Assign(
                                        instanceVariable, Expression.Convert(
                                            Expression.MakeMemberAccess(entryVariable, _entityMemberInfo),
                                            entityType.ClrType))),
                                MaterializeEntity(
                                    entityType, materializationContextVariable, concreteEntityTypeVariable, instanceVariable,
                                    entryVariable))));
                }
                else
                {
                    expressions.Add(
                        Expression.IfThen(
                            primaryKey != null
                                ? primaryKey.Properties.Select(
                                        p =>
                                            Expression.NotEqual(
                                                _entityMaterializerSource.CreateReadValueExpression(
                                                    valueBufferExpression,
                                                    typeof(object),
                                                    p.GetIndex(),
                                                    p),
                                                Expression.Constant(null)))
                                    .Aggregate((a, b) => Expression.AndAlso(a, b))
                                : entityType.GetProperties()
                                    .Select(
                                        p =>
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

                var shadowValuesVariable = Expression.Variable(
                    typeof(ValueBuffer),
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
                    var discriminatorValueVariable = Expression.Variable(
                        discriminatorProperty.ClrType, "discriminator" + _currentEntityIndex);
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

                if (_trackQueryResults
                    && entityType.FindPrimaryKey() != null)
                {
                    foreach (var et in entityType.GetTypesInHierarchy())
                    {
                        _visitedEntityTypes.Add(et);
                    }

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
                    var valueBufferExpression = Expression.Call(
                        materializationContextVariable, MaterializationContext.GetValueBufferMethod);
                    var shadowProperties = concreteEntityType.GetProperties().Where(p => p.IsShadowProperty());
                    blockExpressions.Add(
                        Expression.Assign(
                            shadowValuesVariable,
                            Expression.New(
                                _valueBufferConstructor,
                                Expression.NewArrayInit(
                                    typeof(object),
                                    shadowProperties.Select(
                                        p => _entityMaterializerSource.CreateReadValueExpression(
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
