// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryQueryExpression : Expression, IPrintableExpression
    {
        private static readonly ConstructorInfo _valueBufferConstructor
            = typeof(ValueBuffer).GetConstructors().Single(ci => ci.GetParameters().Length == 1);

        private static readonly PropertyInfo _valueBufferCountMemberInfo
            = typeof(ValueBuffer).GetTypeInfo().GetProperty(nameof(ValueBuffer.Count));

        private readonly List<Expression> _valueBufferSlots = new List<Expression>();

        private readonly IDictionary<EntityProjectionExpression, IDictionary<IProperty, int>> _entityProjectionCache
            = new Dictionary<EntityProjectionExpression, IDictionary<IProperty, int>>();

        private readonly ParameterExpression _valueBufferParameter;

        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
        private ParameterExpression _groupingParameter;

        public virtual IReadOnlyList<Expression> Projection => _valueBufferSlots;
        public virtual Expression ServerQueryExpression { get; set; }
        public virtual ParameterExpression CurrentParameter => _groupingParameter ?? _valueBufferParameter;
        public override Type Type => typeof(IEnumerable<ValueBuffer>);
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        public InMemoryQueryExpression(IEntityType entityType)
        {
            _valueBufferParameter = Parameter(typeof(ValueBuffer), "valueBuffer");
            ServerQueryExpression = new InMemoryTableExpression(entityType);
            var readExpressionMap = new Dictionary<IProperty, Expression>();
            foreach (var property in entityType.GetAllBaseTypesInclusive().SelectMany(et => et.GetDeclaredProperties()))
            {
                readExpressionMap[property] = CreateReadValueExpression(property.ClrType, property.GetIndex(), property);
            }

            foreach (var property in entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredProperties()))
            {
                readExpressionMap[property] = Condition(
                    LessThan(
                        Constant(property.GetIndex()),
                        MakeMemberAccess(
                            _valueBufferParameter,
                            _valueBufferCountMemberInfo)),
                    CreateReadValueExpression(property.ClrType, property.GetIndex(), property),
                    Default(property.ClrType));
            }

            var entityProjection = new EntityProjectionExpression(entityType, readExpressionMap);
            _projectionMapping[new ProjectionMember()] = entityProjection;
        }

        public virtual Expression GetSingleScalarProjection()
        {
            var expression = CreateReadValueExpression(ServerQueryExpression.Type, 0, null);
            _projectionMapping.Clear();
            _projectionMapping[new ProjectionMember()] = expression;

            ConvertToEnumerable();

            return new ProjectionBindingExpression(this, new ProjectionMember(), expression.Type);
        }

        public virtual void ConvertToEnumerable()
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
                                NewArrayInit(typeof(object), ServerQueryExpression))));
                }
                else
                {
                    ServerQueryExpression = New(
                        typeof(ResultEnumerable).GetConstructors().Single(),
                        Lambda<Func<ValueBuffer>>(ServerQueryExpression));
                }
            }
        }

        public virtual void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMappings)
        {
            _projectionMapping.Clear();
            foreach (var kvp in projectionMappings)
            {
                _projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        public virtual IDictionary<IProperty, int> AddToProjection(EntityProjectionExpression entityProjectionExpression)
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

        public virtual int AddToProjection(Expression expression)
        {
            _valueBufferSlots.Add(expression);

            return _valueBufferSlots.Count - 1;
        }

        public virtual int AddSubqueryProjection(ShapedQueryExpression shapedQueryExpression, out Expression innerShaper)
        {
            var subquery = (InMemoryQueryExpression)shapedQueryExpression.QueryExpression;
            subquery.ApplyProjection();
            var serverQueryExpression = subquery.ServerQueryExpression;

            if (serverQueryExpression is MethodCallExpression selectMethodCall
                && selectMethodCall.Arguments[0].Type == typeof(ResultEnumerable))
            {
                var terminatingMethodCall =
                    (MethodCallExpression)((LambdaExpression)((NewExpression)selectMethodCall.Arguments[0]).Arguments[0]).Body;
                selectMethodCall = selectMethodCall.Update(
                    null, new[] { terminatingMethodCall.Arguments[0], selectMethodCall.Arguments[1] });
                serverQueryExpression = terminatingMethodCall.Update(null, new[] { selectMethodCall });
            }

            innerShaper = new ShaperRemappingExpressionVisitor(subquery._projectionMapping)
                .Visit(shapedQueryExpression.ShaperExpression);

            innerShaper = Lambda(innerShaper, subquery.CurrentParameter);

            return AddToProjection(serverQueryExpression);
        }

        private class ShaperRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly IDictionary<ProjectionMember, Expression> _projectionMapping;

            public ShaperRemappingExpressionVisitor(IDictionary<ProjectionMember, Expression> projectionMapping)
            {
                _projectionMapping = projectionMapping;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression
                    && projectionBindingExpression.ProjectionMember != null)
                {
                    var mappingValue = ((ConstantExpression)_projectionMapping[projectionBindingExpression.ProjectionMember]).Value;
                    if (mappingValue is IDictionary<IProperty, int> indexMap)
                    {
                        return new ProjectionBindingExpression(projectionBindingExpression.QueryExpression, indexMap);
                    }

                    if (mappingValue is int index)
                    {
                        return new ProjectionBindingExpression(
                            projectionBindingExpression.QueryExpression, index, projectionBindingExpression.Type);
                    }

                    throw new InvalidOperationException("Invalid ProjectionMapping.");
                }

                return base.Visit(expression);
            }
        }

        private IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetTypesInHierarchy().SelectMany(EntityTypeExtensions.GetDeclaredProperties);

        public virtual Expression GetMappedProjection(ProjectionMember member)
            => _projectionMapping[member];

        public virtual void PushdownIntoSubquery()
        {
            var clientProjection = _valueBufferSlots.Count != 0;
            if (!clientProjection)
            {
                var result = new Dictionary<ProjectionMember, Expression>();
                foreach (var keyValuePair in _projectionMapping)
                {
                    if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                    {
                        var map = new Dictionary<IProperty, Expression>();
                        foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                        {
                            var expressionToAdd = entityProjection.BindProperty(property);
                            var index = AddToProjection(expressionToAdd);
                            map[property] = CreateReadValueExpression(expressionToAdd.Type, index, property);
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
            }

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        _valueBufferSlots
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                CurrentParameter);

            _groupingParameter = null;

            ServerQueryExpression = Call(
                EnumerableMethods.Select.MakeGenericMethod(ServerQueryExpression.Type.TryGetSequenceType(), typeof(ValueBuffer)),
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

        public virtual void ApplyDefaultIfEmpty()
        {
            if (_valueBufferSlots.Count != 0)
            {
                throw new InvalidOperationException("Cannot apply DefaultIfEmpty after a client-evaluated projection.");
            }

            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    var map = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var expressionToAdd = entityProjection.BindProperty(property);
                        var index = AddToProjection(expressionToAdd);
                        map[property] = CreateReadValueExpression(expressionToAdd.Type.MakeNullable(), index, property);
                    }

                    result[keyValuePair.Key] = new EntityProjectionExpression(entityProjection.EntityType, map);
                }
                else
                {
                    var index = AddToProjection(keyValuePair.Value);
                    result[keyValuePair.Key] = CreateReadValueExpression(
                        keyValuePair.Value.Type.MakeNullable(), index, InferPropertyFromInner(keyValuePair.Value));
                }
            }

            _projectionMapping = result;

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        _valueBufferSlots
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                CurrentParameter);

            _groupingParameter = null;

            ServerQueryExpression = Call(
                EnumerableMethods.Select.MakeGenericMethod(ServerQueryExpression.Type.TryGetSequenceType(), typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);

            ServerQueryExpression = Call(
                EnumerableMethods.DefaultIfEmptyWithArgument.MakeGenericMethod(typeof(ValueBuffer)),
                ServerQueryExpression,
                New(_valueBufferConstructor, NewArrayInit(typeof(object), Enumerable.Repeat(Constant(null), _valueBufferSlots.Count))));

            _valueBufferSlots.Clear();
        }

        private static IPropertyBase InferPropertyFromInner(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
            {
                return (IPropertyBase)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
            }

            return null;
        }

        public virtual void ApplyProjection()
        {
            if (_valueBufferSlots.Count == 0)
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
            }

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        _valueBufferSlots
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e)
                            .ToArray())),
                CurrentParameter);

            ServerQueryExpression = Call(
                EnumerableMethods.Select.MakeGenericMethod(typeof(ValueBuffer), typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);
        }

        public virtual InMemoryGroupByShaperExpression ApplyGrouping(Expression groupingKey, Expression shaperExpression)
        {
            PushdownIntoSubquery();

            var selectMethod = (MethodCallExpression)ServerQueryExpression;
            var groupBySource = selectMethod.Arguments[0];
            var elementSelector = selectMethod.Arguments[1];
            _groupingParameter = Parameter(typeof(IGrouping<ValueBuffer, ValueBuffer>), "grouping");
            var groupingKeyAccessExpression = PropertyOrField(_groupingParameter, nameof(IGrouping<int, int>.Key));
            var groupingKeyExpressions = new List<Expression>();
            groupingKey = GetGroupingKey(groupingKey, groupingKeyExpressions, groupingKeyAccessExpression);
            var keySelector = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        groupingKeyExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                _valueBufferParameter);

            ServerQueryExpression = Call(
                EnumerableMethods.GroupByWithKeyElementSelector.MakeGenericMethod(
                    typeof(ValueBuffer), typeof(ValueBuffer), typeof(ValueBuffer)),
                selectMethod.Arguments[0],
                keySelector,
                selectMethod.Arguments[1]);

            return new InMemoryGroupByShaperExpression(
                groupingKey,
                shaperExpression,
                _groupingParameter,
                _valueBufferParameter);
        }

        private Expression GetGroupingKey(Expression key, List<Expression> groupingExpressions, Expression groupingKeyAccessExpression)
        {
            switch (key)
            {
                case NewExpression newExpression:
                    // For .NET Framework only. If ctor is null that means the type is struct and has no ctor args.
                    if (newExpression.Constructor == null)
                    {
                        return newExpression;
                    }

                    var arguments = new Expression[newExpression.Arguments.Count];
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        arguments[i] = GetGroupingKey(newExpression.Arguments[i], groupingExpressions, groupingKeyAccessExpression);
                    }

                    return newExpression.Update(arguments);

                case MemberInitExpression memberInitExpression:
                    if (memberInitExpression.Bindings.Any(mb => !(mb is MemberAssignment)))
                    {
                        goto default;
                    }

                    var updatedNewExpression = (NewExpression)GetGroupingKey(
                        memberInitExpression.NewExpression, groupingExpressions, groupingKeyAccessExpression);
                    var memberBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
                    for (var i = 0; i < memberBindings.Length; i++)
                    {
                        var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                        memberBindings[i] = memberAssignment.Update(
                            GetGroupingKey(
                                memberAssignment.Expression,
                                groupingExpressions,
                                groupingKeyAccessExpression));
                    }

                    return memberInitExpression.Update(updatedNewExpression, memberBindings);

                default:
                    var index = groupingExpressions.Count;
                    groupingExpressions.Add(key);
                    return CreateReadValueExpression(
                        groupingKeyAccessExpression,
                        key.Type,
                        index,
                        InferPropertyFromInner(key));
            }
        }

        private static Expression CreateReadValueExpression(
            Expression valueBufferParameter, Type type, int index, IPropertyBase property)
            => Call(
                EntityMaterializerSource.TryReadValueMethod.MakeGenericMethod(type),
                valueBufferParameter,
                Constant(index),
                Constant(property, typeof(IPropertyBase)));

        private Expression CreateReadValueExpression(Type type, int index, IPropertyBase property)
            => CreateReadValueExpression(_valueBufferParameter, type, index, property);

        public virtual void AddInnerJoin(
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
                    { CurrentParameter, outerParameter }, { innerQueryExpression.CurrentParameter, innerParameter }
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
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        resultValueBufferExpressions.Add(replacedExpression);
                        readExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
                    }

                    projectionMapping[projection.Key.Prepend(outerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    resultValueBufferExpressions.Add(replacingVisitor.Visit(projection.Value));
                    projectionMapping[projection.Key.Prepend(outerMemberInfo)]
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
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        resultValueBufferExpressions.Add(replacedExpression);
                        readExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
                    }

                    projectionMapping[projection.Key.Prepend(innerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    resultValueBufferExpressions.Add(replacingVisitor.Visit(projection.Value));
                    projectionMapping[projection.Key.Prepend(innerMemberInfo)]
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
                EnumerableMethods.Join.MakeGenericMethod(
                    typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
                ServerQueryExpression,
                innerQueryExpression.ServerQueryExpression,
                outerKeySelector,
                innerKeySelector,
                resultSelector);

            _projectionMapping = projectionMapping;
        }

        public virtual void AddLeftJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            Type transparentIdentifierType)
        {
            // GroupJoin phase
            var groupTransparentIdentifierType = TransparentIdentifierFactory.Create(
                typeof(ValueBuffer), typeof(IEnumerable<ValueBuffer>));
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(IEnumerable<ValueBuffer>), "inner");
            var outerMemberInfo = groupTransparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var innerMemberInfo = groupTransparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            var resultSelector = Lambda(
                New(
                    groupTransparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
                    new[] { outerParameter, innerParameter }, outerMemberInfo, innerMemberInfo),
                outerParameter,
                innerParameter);

            var groupJoinExpression = Call(
                EnumerableMethods.GroupJoin.MakeGenericMethod(
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
                    { CurrentParameter, MakeMemberAccess(outerParameter, outerMemberInfo) },
                    { innerQueryExpression.CurrentParameter, innerParameter }
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

                    projectionMapping[projection.Key.Prepend(outerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    resultValueBufferExpressions.Add(replacedExpression);
                    projectionMapping[projection.Key.Prepend(outerMemberInfo)]
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

                    projectionMapping[projection.Key.Prepend(innerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    replacedExpression = nullableReadValueExpressionVisitor.Visit(replacedExpression);
                    resultValueBufferExpressions.Add(replacedExpression);
                    projectionMapping[projection.Key.Prepend(innerMemberInfo)]
                        = CreateReadValueExpression(replacedExpression.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var collectionSelector = Lambda(
                Call(
                    EnumerableMethods.DefaultIfEmptyWithArgument.MakeGenericMethod(typeof(ValueBuffer)),
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
                EnumerableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                    groupTransparentIdentifierType, typeof(ValueBuffer), typeof(ValueBuffer)),
                groupJoinExpression,
                collectionSelector,
                resultSelector);

            _projectionMapping = projectionMapping;
        }

        public virtual void AddSelectMany(InMemoryQueryExpression innerQueryExpression, Type transparentIdentifierType, bool innerNullable)
        {
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(ValueBuffer), "inner");
            var resultValueBufferExpressions = new List<Expression>();
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            var replacingVisitor = new ReplacingExpressionVisitor(
                new Dictionary<Expression, Expression>
                {
                    { CurrentParameter, outerParameter }, { innerQueryExpression.CurrentParameter, innerParameter }
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
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        resultValueBufferExpressions.Add(replacedExpression);
                        readExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
                    }

                    projectionMapping[projection.Key.Prepend(outerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    resultValueBufferExpressions.Add(replacingVisitor.Visit(projection.Value));
                    projectionMapping[projection.Key.Prepend(outerMemberInfo)]
                        = CreateReadValueExpression(projection.Value.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            var nullableReadValueExpressionVisitor = new NullableReadValueExpressionVisitor();
            foreach (var projection in innerQueryExpression._projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, Expression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        if (innerNullable)
                        {
                            replacedExpression = nullableReadValueExpressionVisitor.Visit(replacedExpression);
                        }

                        resultValueBufferExpressions.Add(replacedExpression);
                        readExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
                    }

                    projectionMapping[projection.Key.Prepend(innerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    if (innerNullable)
                    {
                        replacedExpression = nullableReadValueExpressionVisitor.Visit(replacedExpression);
                    }

                    resultValueBufferExpressions.Add(replacedExpression);
                    projectionMapping[projection.Key.Prepend(innerMemberInfo)]
                        = CreateReadValueExpression(replacedExpression.Type, index++, InferPropertyFromInner(projection.Value));
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
                EnumerableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                    typeof(ValueBuffer), typeof(ValueBuffer), typeof(ValueBuffer)),
                ServerQueryExpression,
                Lambda(innerQueryExpression.ServerQueryExpression, CurrentParameter),
                resultSelector);

            _projectionMapping = projectionMapping;
        }

        public virtual EntityShaperExpression AddNavigationToWeakEntityType(
            EntityProjectionExpression entityProjectionExpression,
            INavigation navigation,
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector)
        {
            // GroupJoin phase
            var groupTransparentIdentifierType = TransparentIdentifierFactory.Create(
                typeof(ValueBuffer), typeof(IEnumerable<ValueBuffer>));
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(IEnumerable<ValueBuffer>), "inner");
            var outerMemberInfo = groupTransparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var innerMemberInfo = groupTransparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            var resultSelector = Lambda(
                New(
                    groupTransparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
                    new[] { outerParameter, innerParameter }, outerMemberInfo, innerMemberInfo),
                outerParameter,
                innerParameter);

            var groupJoinExpression = Call(
                EnumerableMethods.GroupJoin.MakeGenericMethod(
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
                    { CurrentParameter, MakeMemberAccess(outerParameter, outerMemberInfo) },
                    { innerQueryExpression.CurrentParameter, innerParameter }
                });
            var index = 0;

            EntityProjectionExpression copyEntityProjectionToOuter(EntityProjectionExpression entityProjection)
            {
                var readExpressionMap = new Dictionary<IProperty, Expression>();
                foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                {
                    var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                    resultValueBufferExpressions.Add(replacedExpression);
                    readExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
                }

                var newEntityProjection = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                if (ReferenceEquals(entityProjectionExpression, entityProjection))
                {
                    entityProjectionExpression = newEntityProjection;
                }

                // Also lift nested entity projections
                foreach (var navigation in entityProjection.EntityType.GetTypesInHierarchy()
                    .SelectMany(EntityTypeExtensions.GetDeclaredNavigations))
                {
                    var boundEntityShaperExpression = entityProjection.BindNavigation(navigation);
                    if (boundEntityShaperExpression != null)
                    {
                        var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                        var newInnerEntityProjection = copyEntityProjectionToOuter(innerEntityProjection);
                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                        newEntityProjection.AddNavigationBinding(navigation, boundEntityShaperExpression);
                    }
                }

                return newEntityProjection;
            }

            foreach (var projection in _projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    projectionMapping[projection.Key] = copyEntityProjectionToOuter(entityProjection);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    resultValueBufferExpressions.Add(replacedExpression);
                    projectionMapping[projection.Key]
                        = CreateReadValueExpression(replacedExpression.Type, index++, InferPropertyFromInner(projection.Value));
                }
            }

            _projectionMapping = projectionMapping;

            var outerIndex = index;
            var nullableReadValueExpressionVisitor = new NullableReadValueExpressionVisitor();
            var innerEntityProjection = (EntityProjectionExpression)innerQueryExpression.GetMappedProjection(new ProjectionMember());

            var innerReadExpressionMap = new Dictionary<IProperty, Expression>();
            foreach (var property in GetAllPropertiesInHierarchy(innerEntityProjection.EntityType))
            {
                var replacedExpression = replacingVisitor.Visit(innerEntityProjection.BindProperty(property));
                replacedExpression = nullableReadValueExpressionVisitor.Visit(replacedExpression);
                resultValueBufferExpressions.Add(replacedExpression);
                innerReadExpressionMap[property] = CreateReadValueExpression(replacedExpression.Type, index++, property);
            }

            innerEntityProjection = new EntityProjectionExpression(innerEntityProjection.EntityType, innerReadExpressionMap);

            var collectionSelector = Lambda(
                Call(
                    EnumerableMethods.DefaultIfEmptyWithArgument.MakeGenericMethod(typeof(ValueBuffer)),
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
                EnumerableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                    groupTransparentIdentifierType, typeof(ValueBuffer), typeof(ValueBuffer)),
                groupJoinExpression,
                collectionSelector,
                resultSelector);

            var entityShaper = new EntityShaperExpression(innerEntityProjection.EntityType, innerEntityProjection, nullable: true);
            entityProjectionExpression.AddNavigationBinding(navigation, entityShaper);

            return entityShaper;
        }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine(nameof(InMemoryQueryExpression) + ": ");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine(nameof(ServerQueryExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(ServerQueryExpression);
                }

                expressionPrinter.AppendLine("ProjectionMapping:");
                using (expressionPrinter.Indent())
                {
                    foreach (var projectionMapping in _projectionMapping)
                    {
                        expressionPrinter.Append("Member: " + projectionMapping.Key + " Projection: ");
                        expressionPrinter.Visit(projectionMapping.Value);
                    }
                }

                expressionPrinter.AppendLine();
            }
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

                return Condition(test, ifTrue, ifFalse);
            }
        }
    }
}
