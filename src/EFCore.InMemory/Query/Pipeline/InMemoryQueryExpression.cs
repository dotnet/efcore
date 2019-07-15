// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryQueryExpression : Expression
    {
        private static readonly ConstructorInfo _valueBufferConstructor
            = typeof(ValueBuffer).GetConstructors().Single(ci => ci.GetParameters().Length == 1);

        private readonly List<Expression> _valueBufferSlots = new List<Expression>();
        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();

        public IReadOnlyList<Expression> Projection => _valueBufferSlots;
        private readonly IDictionary<EntityProjectionExpression, IDictionary<IProperty, int>> _entityProjectionCache
            = new Dictionary<EntityProjectionExpression, IDictionary<IProperty, int>>();

        private sealed class ResultEnumerable : IEnumerable<ValueBuffer>
        {
            private readonly Func<ValueBuffer> _getElement;

            public ResultEnumerable(Func<ValueBuffer> getElement)
            {
                _getElement = getElement;
            }

            public IEnumerator<ValueBuffer> GetEnumerator() => new ResultEnumerator(_getElement());

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class ResultEnumerator : IEnumerator<ValueBuffer>
            {
                private readonly ValueBuffer _value;
                private bool _moved;

                public ResultEnumerator(ValueBuffer value) => _value = value;

                public bool MoveNext()
                {
                    if (!_moved)
                    {
                        _moved = true;

                        return _moved;
                    }

                    return false;
                }

                public void Reset()
                {
                    _moved = false;
                }

                object IEnumerator.Current => Current;

                public ValueBuffer Current => !_moved ? ValueBuffer.Empty : _value;

                void IDisposable.Dispose()
                {
                }
            }
        }

        private static readonly PropertyInfo _valueBufferCountMemberInfo = typeof(ValueBuffer).GetTypeInfo().GetProperty(nameof(ValueBuffer.Count));

        public InMemoryQueryExpression(IEntityType entityType)
        {
            ValueBufferParameter = Parameter(typeof(ValueBuffer), "valueBuffer");
            ServerQueryExpression = new InMemoryTableExpression(entityType);
            var readExpressionMap = new Dictionary<IProperty, Expression>();
            foreach (var property in entityType.GetAllBaseTypesInclusive().SelectMany(et => et.GetDeclaredProperties()))
            {
                readExpressionMap[property] = CreateReadValueExpression(property.ClrType, property.GetIndex(), property);
            }

            foreach (var property in entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredProperties()))
            {
                readExpressionMap[property] = Condition(
                    LessThan(Constant(property.GetIndex()), MakeMemberAccess(ValueBufferParameter, _valueBufferCountMemberInfo)),
                    CreateReadValueExpression(property.ClrType, property.GetIndex(), property),
                    Default(property.ClrType));
            }

            var entityProjection = new EntityProjectionExpression(entityType, readExpressionMap);
            _projectionMapping[new ProjectionMember()] = entityProjection;
        }

        public Expression GetSingleScalarProjection()
        {
            var expression = CreateReadValueExpression(ServerQueryExpression.Type, 0, null);
            _projectionMapping.Clear();
            _projectionMapping[new ProjectionMember()] = expression;

            ConvertToEnumerable();

            return new ProjectionBindingExpression(this, new ProjectionMember(), expression.Type);
        }

        public void ConvertToEnumerable()
        {
            if (ServerQueryExpression.Type.TryGetSequenceType() == null)
            {
                if (ServerQueryExpression.Type != typeof(ValueBuffer))
                {
                    if (ServerQueryExpression.Type.IsValueType)
                    {
                        ServerQueryExpression = Convert(ServerQueryExpression, typeof(object));
                    }

                    ServerQueryExpression = New(
                       typeof(ResultEnumerable).GetConstructors().Single(),
                       Lambda<Func<ValueBuffer>>(
                           New(
                               _valueBufferConstructor,
                               NewArrayInit(typeof(object), new[] { ServerQueryExpression }))));
                }
                else
                {
                    ServerQueryExpression = New(
                        typeof(ResultEnumerable).GetConstructors().Single(),
                        Lambda<Func<ValueBuffer>>(ServerQueryExpression));
                }
            }
        }

        public void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMappings)
        {
            _projectionMapping.Clear();
            foreach (var kvp in projectionMappings)
            {
                _projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        public IDictionary<IProperty, int> AddToProjection(EntityProjectionExpression entityProjectionExpression)
        {
            if (!_entityProjectionCache.TryGetValue(entityProjectionExpression, out var indexMap))
            {
                indexMap = new Dictionary<IProperty, int>();
                foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
                {
                    indexMap[property] = AddToProjection(entityProjectionExpression.BindProperty(property));
                }

                _entityProjectionCache[entityProjectionExpression] = indexMap;
            }

            return indexMap;
        }

        public int AddToProjection(Expression expression)
        {
            _valueBufferSlots.Add(expression);

            return _valueBufferSlots.Count - 1;
        }

        private IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetTypesInHierarchy().SelectMany(EntityTypeExtensions.GetDeclaredProperties);

        public Expression GetMappedProjection(ProjectionMember member)
            => _projectionMapping[member];

        public void ApplyPendingSelector()
        {
            var clientProjection = _valueBufferSlots.Count != 0;
            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    var map = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var index = AddToProjection(entityProjection.BindProperty(property));
                        map[property] = CreateReadValueExpression(property.ClrType, index, property);
                    }
                    result[keyValuePair.Key] = new EntityProjectionExpression(entityProjection.EntityType, map);
                }
                else
                {
                    var index = AddToProjection(keyValuePair.Value);
                    result[keyValuePair.Key] = CreateReadValueExpression(
                        keyValuePair.Value.Type, index, InferPropertyFromInner(keyValuePair.Value));
                }
            }

            _projectionMapping = result;

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        _valueBufferSlots
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e)
                            .ToArray())),
                ValueBufferParameter);

            ServerQueryExpression = Call(
                InMemoryLinqOperatorProvider.Select.MakeGenericMethod(typeof(ValueBuffer), typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);

            if (clientProjection)
            {
                var newValueBufferSlots = _valueBufferSlots
                    .Select((e, i) => CreateReadValueExpression(e.Type, i, InferPropertyFromInner(e)))
                    .ToList();

                _valueBufferSlots.Clear();
                _valueBufferSlots.AddRange(newValueBufferSlots);
            }
            else
            {
                _valueBufferSlots.Clear();
            }
        }

        private IPropertyBase InferPropertyFromInner(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
            {
                return (IPropertyBase)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
            }

            return null;
        }

        public void ApplyServerProjection()
        {
            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    var map = new Dictionary<IProperty, int>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        map[property] = AddToProjection(entityProjection.BindProperty(property));
                    }
                    result[keyValuePair.Key] = Constant(map);
                }
                else
                {
                    result[keyValuePair.Key] = Constant(AddToProjection(keyValuePair.Value));
                }
            }

            _projectionMapping = result;

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        _valueBufferSlots
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e)
                            .ToArray())),
                ValueBufferParameter);

            ServerQueryExpression = Call(
                InMemoryLinqOperatorProvider.Select.MakeGenericMethod(typeof(ValueBuffer), typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);
        }

        public Expression ServerQueryExpression { get; set; }
        public ParameterExpression ValueBufferParameter { get; }
        public override Type Type => typeof(IEnumerable<ValueBuffer>);
        public override ExpressionType NodeType => ExpressionType.Extension;

        private Expression CreateReadValueExpression(
            Type type,
            int index,
            IPropertyBase property)
            => Call(
                EntityMaterializerSource.TryReadValueMethod.MakeGenericMethod(type),
                ValueBufferParameter,
                Constant(index),
                Constant(property, typeof(IPropertyBase)));

        public void AddInnerJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            Type transparentIdentifierType)
        {
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(ValueBuffer), "inner");
            var resultValueBufferExpressions = new List<Expression>();
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            var replacingVisitor = new ReplacingExpressionVisitor(
                    new Dictionary<Expression, Expression>
                    {
                        { ValueBufferParameter, outerParameter },
                        { innerQueryExpression.ValueBufferParameter, innerParameter }
                    });

            var index = 0;
            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            foreach (var projection in _projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        resultValueBufferExpressions.Add(replacingVisitor.Visit(entityProjection.BindProperty(property)));
                        readExpressionMap[property] = CreateReadValueExpression(property.ClrType, index++, property);
                    }
                    projectionMapping[projection.Key.ShiftMember(outerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    resultValueBufferExpressions.Add(replacingVisitor.Visit(projection.Value));
                    projectionMapping[projection.Key.ShiftMember(outerMemberInfo)]
                        = CreateReadValueExpression(projection.Value.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            foreach (var projection in innerQueryExpression._projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        resultValueBufferExpressions.Add(replacingVisitor.Visit(entityProjection.BindProperty(property)));
                        readExpressionMap[property] = CreateReadValueExpression(property.ClrType, index++, property);
                    }
                    projectionMapping[projection.Key.ShiftMember(innerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    resultValueBufferExpressions.Add(replacingVisitor.Visit(projection.Value));
                    projectionMapping[projection.Key.ShiftMember(innerMemberInfo)]
                        = CreateReadValueExpression(projection.Value.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var resultSelector = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        resultValueBufferExpressions
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e)
                            .ToArray())),
                outerParameter,
                innerParameter);

            ServerQueryExpression = Call(
                InMemoryLinqOperatorProvider.Join.MakeGenericMethod(
                    typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
                ServerQueryExpression,
                innerQueryExpression.ServerQueryExpression,
                outerKeySelector,
                innerKeySelector,
                resultSelector);

            _projectionMapping = projectionMapping;
        }

        public readonly struct TransparentIdentifier<TOuter, TInner>
        {
            [UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members
            private TransparentIdentifier(TOuter outer, TInner inner)
#pragma warning restore IDE0051 // Remove unused private members
            {
                Outer = outer;
                Inner = inner;
            }

            [UsedImplicitly]
            public readonly TOuter Outer;

            [UsedImplicitly]
            public readonly TInner Inner;
        }

        public void AddLeftJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            Type transparentIdentifierType)
        {
            // GroupJoin phase
            var groupTransparentIdentifierType = typeof(TransparentIdentifier<,>)
                .MakeGenericType(typeof(ValueBuffer), typeof(IEnumerable<ValueBuffer>));
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(IEnumerable<ValueBuffer>), "inner");
            var outerMemberInfo = groupTransparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var innerMemberInfo = groupTransparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            var resultSelector = Lambda(
                New(
                    groupTransparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
                    new[] { outerParameter, innerParameter },
                    new[] { outerMemberInfo, innerMemberInfo }),
                outerParameter,
                innerParameter);

            var groupJoinExpression = Call(
                InMemoryLinqOperatorProvider.GroupJoin.MakeGenericMethod(
                    typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, groupTransparentIdentifierType),
                ServerQueryExpression,
                innerQueryExpression.ServerQueryExpression,
                outerKeySelector,
                innerKeySelector,
                resultSelector);

            // SelectMany phase
            var collectionParameter = Parameter(groupTransparentIdentifierType, "collection");
            var collection = MakeMemberAccess(collectionParameter, innerMemberInfo);
            outerParameter = Parameter(groupTransparentIdentifierType, "outer");
            innerParameter = Parameter(typeof(ValueBuffer), "inner");

            var resultValueBufferExpressions = new List<Expression>();
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            var replacingVisitor = new ReplacingExpressionVisitor(
                    new Dictionary<Expression, Expression>
                    {
                        { ValueBufferParameter, MakeMemberAccess(outerParameter, outerMemberInfo) },
                        { innerQueryExpression.ValueBufferParameter, innerParameter }
                    });

            var index = 0;
            outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            foreach (var projection in _projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        resultValueBufferExpressions.Add(replacedExpression);
                        readExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
                    }
                    projectionMapping[projection.Key.ShiftMember(outerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    resultValueBufferExpressions.Add(replacedExpression);
                    projectionMapping[projection.Key.ShiftMember(outerMemberInfo)]
                        = CreateReadValueExpression(replacedExpression.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var outerIndex = index;
            innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            var nullableReadValueExpressionVisitor = new NullableReadValueExpressionVisitor();
            foreach (var projection in innerQueryExpression._projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        replacedExpression = nullableReadValueExpressionVisitor.Visit(replacedExpression);
                        resultValueBufferExpressions.Add(replacedExpression);
                        readExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
                    }
                    projectionMapping[projection.Key.ShiftMember(innerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    replacedExpression = nullableReadValueExpressionVisitor.Visit(replacedExpression);
                    resultValueBufferExpressions.Add(replacedExpression);
                    projectionMapping[projection.Key.ShiftMember(innerMemberInfo)]
                        = CreateReadValueExpression(replacedExpression.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var collectionSelector = Lambda(
                Call(
                    InMemoryLinqOperatorProvider.DefaultIfEmptyWithArg.MakeGenericMethod(typeof(ValueBuffer)),
                    collection,
                    New(
                        _valueBufferConstructor,
                        NewArrayInit(
                            typeof(object),
                            Enumerable.Range(0, index - outerIndex).Select(i => Constant(null))))),
                collectionParameter);

            resultSelector = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        resultValueBufferExpressions
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e)
                            .ToArray())),
                outerParameter,
                innerParameter);

            ServerQueryExpression = Call(
                InMemoryLinqOperatorProvider.SelectMany.MakeGenericMethod(
                    groupTransparentIdentifierType, typeof(ValueBuffer), typeof(ValueBuffer)),
                groupJoinExpression,
                collectionSelector,
                resultSelector);

            _projectionMapping = projectionMapping;
        }

        public void AddCrossJoin(InMemoryQueryExpression innerQueryExpression, Type transparentIdentifierType)
        {
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(ValueBuffer), "inner");
            var resultValueBufferExpressions = new List<Expression>();
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            var replacingVisitor = new ReplacingExpressionVisitor(
                    new Dictionary<Expression, Expression>
                    {
                        { ValueBufferParameter, outerParameter },
                        { innerQueryExpression.ValueBufferParameter, innerParameter }
                    });

            var index = 0;
            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            foreach (var projection in _projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        resultValueBufferExpressions.Add(replacingVisitor.Visit(entityProjection.BindProperty(property)));
                        readExpressionMap[property] = CreateReadValueExpression(property.ClrType, index++, property);
                    }
                    projectionMapping[projection.Key.ShiftMember(outerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    resultValueBufferExpressions.Add(replacingVisitor.Visit(projection.Value));
                    projectionMapping[projection.Key.ShiftMember(outerMemberInfo)]
                        = CreateReadValueExpression(projection.Value.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            foreach (var projection in innerQueryExpression._projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        resultValueBufferExpressions.Add(replacingVisitor.Visit(entityProjection.BindProperty(property)));
                        readExpressionMap[property] = CreateReadValueExpression(property.ClrType, index++, property);
                    }
                    projectionMapping[projection.Key.ShiftMember(innerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    resultValueBufferExpressions.Add(replacingVisitor.Visit(projection.Value));
                    projectionMapping[projection.Key.ShiftMember(innerMemberInfo)]
                        = CreateReadValueExpression(projection.Value.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var resultSelector = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        resultValueBufferExpressions
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e)
                            .ToArray())),
                outerParameter,
                innerParameter);

            ServerQueryExpression = Call(
                InMemoryLinqOperatorProvider.SelectMany.MakeGenericMethod(
                    typeof(ValueBuffer), typeof(ValueBuffer), typeof(ValueBuffer)),
                ServerQueryExpression,
                Lambda(innerQueryExpression.ServerQueryExpression, ValueBufferParameter),
                resultSelector);

            _projectionMapping = projectionMapping;
        }

        private class NullableReadValueExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod
                    && !methodCallExpression.Type.IsNullableType())
                {
                    return Call(
                        EntityMaterializerSource.TryReadValueMethod.MakeGenericMethod(methodCallExpression.Type.MakeNullable()),
                        methodCallExpression.Arguments);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
            {
                var test = Visit(conditionalExpression.Test);
                var ifTrue = Visit(conditionalExpression.IfTrue);
                var ifFalse = Visit(conditionalExpression.IfFalse);

                if (ifTrue.Type.IsNullableType()
                    && conditionalExpression.IfTrue.Type == ifTrue.Type.UnwrapNullableType()
                    && ifFalse is DefaultExpression)
                {
                    ifFalse = Default(ifTrue.Type);
                }

                return conditionalExpression.Update(test, ifTrue, ifFalse);
            }

        }
    }
}
