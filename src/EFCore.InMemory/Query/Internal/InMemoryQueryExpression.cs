// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public partial class InMemoryQueryExpression : Expression, IPrintableExpression
    {
        private static readonly ConstructorInfo _valueBufferConstructor
            = typeof(ValueBuffer).GetConstructors().Single(ci => ci.GetParameters().Length == 1);

        private static readonly PropertyInfo _valueBufferCountMemberInfo
            = typeof(ValueBuffer).GetRequiredProperty(nameof(ValueBuffer.Count));

        private static readonly MethodInfo _leftJoinMethodInfo = typeof(InMemoryQueryExpression).GetTypeInfo()
            .GetDeclaredMethods(nameof(LeftJoin)).Single(mi => mi.GetParameters().Length == 6);

        private readonly List<Expression> _clientProjectionExpressions = new();
        private readonly List<MethodCallExpression> _projectionMappingExpressions = new();

        private readonly ParameterExpression _valueBufferParameter;

        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
        private ParameterExpression? _groupingParameter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryQueryExpression(IEntityType entityType)
        {
            _valueBufferParameter = Parameter(typeof(ValueBuffer), "valueBuffer");
            ServerQueryExpression = new InMemoryTableExpression(entityType);
            var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
            var selectorExpressions = new List<Expression>();
            foreach (var property in entityType.GetAllBaseTypesInclusive().SelectMany(et => et.GetDeclaredProperties()))
            {
                var propertyExpression = CreateReadValueExpression(property.ClrType, property.GetIndex(), property);
                selectorExpressions.Add(propertyExpression);

                Check.DebugAssert(property.GetIndex() == selectorExpressions.Count - 1,
                    "Properties should be ordered in same order as their indexes.");
                readExpressionMap[property] = propertyExpression;
                _projectionMappingExpressions.Add(propertyExpression);
            }

            var discriminatorProperty = entityType.FindDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                var keyValueComparer = discriminatorProperty.GetKeyValueComparer()!;
                foreach (var derivedEntityType in entityType.GetDerivedTypes())
                {
                    var entityCheck = derivedEntityType.GetConcreteDerivedTypesInclusive()
                        .Select(
                            e => keyValueComparer.ExtractEqualsBody(
                                readExpressionMap[discriminatorProperty],
                                Constant(e.GetDiscriminatorValue(), discriminatorProperty.ClrType)))
                        .Aggregate((l, r) => OrElse(l, r));

                    foreach (var property in derivedEntityType.GetDeclaredProperties())
                    {
                        var propertyExpression = Condition(
                            entityCheck,
                            CreateReadValueExpression(property.ClrType, property.GetIndex(), property),
                            Default(property.ClrType));

                        selectorExpressions.Add(propertyExpression);
                        var readExpression = CreateReadValueExpression(property.ClrType, selectorExpressions.Count - 1, property);
                        readExpressionMap[property] = readExpression;
                        _projectionMappingExpressions.Add(readExpression);
                    }
                }

                // Force a selector if entity projection has complex expressions.
                var selectorLambda = Lambda(
                    New(
                        _valueBufferConstructor,
                        NewArrayInit(
                            typeof(object),
                            selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                    CurrentParameter);

                ServerQueryExpression = Call(
                    EnumerableMethods.Select.MakeGenericMethod(typeof(ValueBuffer), typeof(ValueBuffer)),
                    ServerQueryExpression,
                    selectorLambda);
            }

            var entityProjection = new EntityProjectionExpression(entityType, readExpressionMap);
            _projectionMapping[new ProjectionMember()] = entityProjection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Expression> Projection
            => _clientProjectionExpressions;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression ServerQueryExpression { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ParameterExpression CurrentParameter
            => _groupingParameter ?? _valueBufferParameter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type
            => typeof(IEnumerable<ValueBuffer>);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression GetSingleScalarProjection()
        {
            var expression = CreateReadValueExpression(ServerQueryExpression.Type, 0, null);
            _projectionMapping.Clear();
            _projectionMapping[new ProjectionMember()] = expression;
            _projectionMappingExpressions.Add(expression);
            _groupingParameter = null;

            ConvertToEnumerable();

            return new ProjectionBindingExpression(this, new ProjectionMember(), expression.Type.MakeNullable());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMappings)
        {
            _projectionMapping.Clear();
            _projectionMappingExpressions.Clear();
            LambdaExpression? selectorLambda = null;
            if (_clientProjectionExpressions.Count > 0)
            {
                var remappedProjections = _clientProjectionExpressions
                    .Select((e, i) => CreateReadValueExpression(e.Type, i, InferPropertyFromInner(e))).ToList();

                selectorLambda = Lambda(
                    New(
                        _valueBufferConstructor,
                        NewArrayInit(
                            typeof(object),
                            _clientProjectionExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                    CurrentParameter);

                _clientProjectionExpressions.Clear();
                _clientProjectionExpressions.AddRange(remappedProjections);
            }
            else
            {
                var selectorExpressions = new List<Expression>();
                foreach (var kvp in projectionMappings)
                {
                    if (kvp.Value is EntityProjectionExpression entityProjectionExpression)
                    {
                        _projectionMapping[kvp.Key] = UpdateEntityProjection(entityProjectionExpression);
                    }
                    else
                    {
                        selectorExpressions.Add(kvp.Value);
                        var expression = CreateReadValueExpression(
                            kvp.Value.Type, selectorExpressions.Count - 1, InferPropertyFromInner(kvp.Value));
                        _projectionMapping[kvp.Key] = expression;
                        _projectionMappingExpressions.Add(expression);
                    }
                }

                if (selectorExpressions.Count == 0)
                {
                    // No server correlated term in projection so add dummy 1.
                    selectorExpressions.Add(Constant(1));
                }

                selectorLambda = Lambda(
                    New(
                        _valueBufferConstructor,
                        NewArrayInit(
                            typeof(object),
                            selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                    CurrentParameter);

                EntityProjectionExpression UpdateEntityProjection(EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var expression = entityProjection.BindProperty(property);
                        selectorExpressions.Add(expression);
                        var newExpression = CreateReadValueExpression(expression.Type, selectorExpressions.Count - 1, property);
                        readExpressionMap[property] = newExpression;
                        _projectionMappingExpressions.Add(newExpression);
                    }

                    var result = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);

                    // Also compute nested entity projections
                    foreach (var navigation in entityProjection.EntityType.GetAllBaseTypes()
                        .Concat(entityProjection.EntityType.GetDerivedTypesInclusive())
                        .SelectMany(t => t.GetDeclaredNavigations()))
                    {
                        var boundEntityShaperExpression = entityProjection.BindNavigation(navigation);
                        if (boundEntityShaperExpression != null)
                        {
                            var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                            var newInnerEntityProjection = UpdateEntityProjection(innerEntityProjection);
                            boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                            result.AddNavigationBinding(navigation, boundEntityShaperExpression);
                        }
                    }

                    return result;
                }
            }

            ServerQueryExpression = Call(
                EnumerableMethods.Select.MakeGenericMethod(CurrentParameter.Type, typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);
            _groupingParameter = null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyDictionary<IProperty, int> AddToProjection(EntityProjectionExpression entityProjectionExpression)
        {
            var indexMap = new Dictionary<IProperty, int>();
            foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
            {
                indexMap[property] = AddToProjection(entityProjectionExpression.BindProperty(property));
            }

            return indexMap;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int AddToProjection(Expression expression)
        {
            _clientProjectionExpressions.Add(expression);

            return _clientProjectionExpressions.Count - 1;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int AddSubqueryProjection(
            ShapedQueryExpression shapedQueryExpression,
            out Expression innerShaper)
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
                    null!, new[] { terminatingMethodCall.Arguments[0], selectMethodCall.Arguments[1] });
                serverQueryExpression = terminatingMethodCall.Update(null!, new[] { selectMethodCall });
            }

            innerShaper = new ShaperRemappingExpressionVisitor(subquery._projectionMapping)
                .Visit(shapedQueryExpression.ShaperExpression);

            innerShaper = Lambda(innerShaper, subquery.CurrentParameter);

            return AddToProjection(serverQueryExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression GetMappedProjection(ProjectionMember member)
            => _projectionMapping[member];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateServerQueryExpression(Expression serverQueryExpression)
            => ServerQueryExpression = serverQueryExpression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplySetOperation(MethodInfo setOperationMethodInfo, InMemoryQueryExpression source2)
        {
            Check.DebugAssert(_groupingParameter == null, "Cannot apply set operation after GroupBy without flattening.");
            if (_clientProjectionExpressions.Count == 0)
            {
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                var source1SelectorExpressions = new List<Expression>();
                var source2SelectorExpressions = new List<Expression>();
                foreach (var (key, value1, value2) in _projectionMapping.Join(
                    source2._projectionMapping, kv => kv.Key, kv => kv.Key,
                    (kv1, kv2) => (kv1.Key, Value1: kv1.Value, Value2: kv2.Value)))
                {
                    if (value1 is EntityProjectionExpression entityProjection1
                        && value2 is EntityProjectionExpression entityProjection2)
                    {
                        var map = new Dictionary<IProperty, MethodCallExpression>();
                        foreach (var property in GetAllPropertiesInHierarchy(entityProjection1.EntityType))
                        {
                            var expressionToAdd1 = entityProjection1.BindProperty(property);
                            var expressionToAdd2 = entityProjection2.BindProperty(property);
                            source1SelectorExpressions.Add(expressionToAdd1);
                            source2SelectorExpressions.Add(expressionToAdd2);
                            var type = expressionToAdd1.Type;
                            if (!type.IsNullableType()
                                && expressionToAdd2.Type.IsNullableType())
                            {
                                type = expressionToAdd2.Type;
                            }
                            map[property] = CreateReadValueExpression(type, source1SelectorExpressions.Count - 1, property);
                        }

                        projectionMapping[key] = new EntityProjectionExpression(entityProjection1.EntityType, map);
                    }
                    else
                    {
                        source1SelectorExpressions.Add(value1);
                        source2SelectorExpressions.Add(value2);
                        var type = value1.Type;
                        if (!type.IsNullableType()
                            && value2.Type.IsNullableType())
                        {
                            type = value2.Type;
                        }
                        projectionMapping[key] = CreateReadValueExpression(type, source1SelectorExpressions.Count - 1, InferPropertyFromInner(value1));
                    }
                }

                _projectionMapping = projectionMapping;

                ServerQueryExpression = Call(
                    EnumerableMethods.Select.MakeGenericMethod(ServerQueryExpression.Type.GetSequenceType(), typeof(ValueBuffer)),
                    ServerQueryExpression,
                    Lambda(
                        New(
                            _valueBufferConstructor,
                            NewArrayInit(
                                typeof(object),
                                source1SelectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                        CurrentParameter));


                source2.ServerQueryExpression = Call(
                    EnumerableMethods.Select.MakeGenericMethod(source2.ServerQueryExpression.Type.GetSequenceType(), typeof(ValueBuffer)),
                    source2.ServerQueryExpression,
                    Lambda(
                    New(
                        _valueBufferConstructor,
                        NewArrayInit(
                            typeof(object),
                            source2SelectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                    source2.CurrentParameter));
            }
            else
            {
                Check.DebugAssert(_clientProjectionExpressions.Count == source2._clientProjectionExpressions.Count,
                    "Index count in both source should match.");

                // In case of client projections, indexes must match so we don't worry about it.
                // We still have to formualte outer client projections again for nullability.
                for (var i = 0; i < source2._clientProjectionExpressions.Count; i++)
                {
                    var type1 = _clientProjectionExpressions[i].Type;
                    var type2 = source2._clientProjectionExpressions[i].Type;
                    if (!type1.IsNullableValueType()
                        && type2.IsNullableValueType())
                    {
                        _clientProjectionExpressions[i] = MakeReadValueNullable(_clientProjectionExpressions[i]);
                    }
                }
            }

            ServerQueryExpression = Call(
                setOperationMethodInfo.MakeGenericMethod(typeof(ValueBuffer)), ServerQueryExpression, source2.ServerQueryExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyDefaultIfEmpty()
        {
            if (_clientProjectionExpressions.Count != 0)
            {
                throw new InvalidOperationException(InMemoryStrings.DefaultIfEmptyAppliedAfterProjection);
            }

            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    var map = new Dictionary<IProperty, MethodCallExpression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        map[property] = MakeReadValueNullable(entityProjection.BindProperty(property));
                    }

                    projectionMapping[keyValuePair.Key] = new EntityProjectionExpression(entityProjection.EntityType, map);
                }
                else
                {
                    projectionMapping[keyValuePair.Key] = MakeReadValueNullable(keyValuePair.Value);
                }
            }

            _projectionMapping = projectionMapping;
            var projectionMappingExpressions = _projectionMappingExpressions.Select(e => MakeReadValueNullable(e)).ToList();
            _projectionMappingExpressions.Clear();
            _projectionMappingExpressions.AddRange(projectionMappingExpressions);

            _groupingParameter = null;

            ServerQueryExpression = Call(
                EnumerableMethods.DefaultIfEmptyWithArgument.MakeGenericMethod(typeof(ValueBuffer)),
                ServerQueryExpression,
                Constant(new ValueBuffer(Enumerable.Repeat((object?)null, _projectionMappingExpressions.Count).ToArray())));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyProjection()
        {
            if (_clientProjectionExpressions.Count == 0)
            {
                var result = new Dictionary<ProjectionMember, Expression>();
                foreach (var keyValuePair in _projectionMapping)
                {
                    result[keyValuePair.Key] = keyValuePair.Value is EntityProjectionExpression entityProjection
                        ? Constant(AddToProjection(entityProjection))
                        : Constant(AddToProjection(keyValuePair.Value));
                }

                _projectionMapping = result;
            }

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        _clientProjectionExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e).ToArray())),
                CurrentParameter);

            ServerQueryExpression = Call(
                EnumerableMethods.Select.MakeGenericMethod(typeof(ValueBuffer), typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InMemoryGroupByShaperExpression ApplyGrouping(
            Expression groupingKey,
            Expression shaperExpression,
            bool defaultElementSelector)
        {
            var source = ServerQueryExpression;
            Expression? selector = null;
            if (defaultElementSelector)
            {
                selector = Lambda(
                    New(
                        _valueBufferConstructor,
                        NewArrayInit(
                            typeof(object),
                            _projectionMappingExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : (Expression)e))),
                    _valueBufferParameter);
            }
            else
            {
                var selectMethodCall = (MethodCallExpression)ServerQueryExpression;
                source = selectMethodCall.Arguments[0];
                selector = selectMethodCall.Arguments[1];
            }

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
                source,
                keySelector,
                selector);

            return new InMemoryGroupByShaperExpression(
                groupingKey,
                shaperExpression,
                _groupingParameter,
                _valueBufferParameter);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddInnerJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            Type transparentIdentifierType)
            => AddJoin(innerQueryExpression, outerKeySelector, innerKeySelector, transparentIdentifierType, innerNullable: false);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddLeftJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            Type transparentIdentifierType)
            => AddJoin(innerQueryExpression, outerKeySelector, innerKeySelector, transparentIdentifierType, innerNullable: true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddSelectMany(
            InMemoryQueryExpression innerQueryExpression,
            Type transparentIdentifierType,
            bool innerNullable)
            => AddJoin(innerQueryExpression, null, null, transparentIdentifierType, innerNullable);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityShaperExpression AddNavigationToWeakEntityType(
            EntityProjectionExpression entityProjectionExpression,
            INavigation navigation,
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector)
        {
            var innerNullable = !navigation.ForeignKey.IsRequiredDependent;
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(ValueBuffer), "inner");
            var replacingVisitor = new ReplacingExpressionVisitor(
                new Expression[] { CurrentParameter, innerQueryExpression.CurrentParameter },
                new Expression[] { outerParameter, innerParameter });
            var resultSelectorExpressions = _projectionMappingExpressions
                .Select(e => replacingVisitor.Visit(e))
                .ToList();

            var outerIndex = resultSelectorExpressions.Count;
            var innerEntityProjection = (EntityProjectionExpression)innerQueryExpression.GetMappedProjection(new ProjectionMember());

            var innerReadExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(innerEntityProjection.EntityType))
            {
                var replacedExpression = replacingVisitor.Visit(innerEntityProjection.BindProperty(property));
                if (innerNullable)
                {
                    replacedExpression = MakeReadValueNullable(replacedExpression);
                }
                resultSelectorExpressions.Add(replacedExpression);
                var readValueExperssion = CreateReadValueExpression(replacedExpression.Type, resultSelectorExpressions.Count - 1, property);
                innerReadExpressionMap[property] = readValueExperssion;
                _projectionMappingExpressions.Add(readValueExperssion);
            }

            innerEntityProjection = new EntityProjectionExpression(innerEntityProjection.EntityType, innerReadExpressionMap);

            var resultSelector = Lambda(
                New(_valueBufferConstructor,
                    NewArrayInit(typeof(object),
                        resultSelectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
                outerParameter,
                innerParameter);

            if (innerNullable)
            {
                ServerQueryExpression = Call(
                    _leftJoinMethodInfo.MakeGenericMethod(
                        typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
                    ServerQueryExpression,
                    innerQueryExpression.ServerQueryExpression,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    Constant(new ValueBuffer(
                        Enumerable.Repeat((object?)null, innerQueryExpression._projectionMappingExpressions.Count).ToArray())));
            }
            else
            {
                ServerQueryExpression = Call(
                    EnumerableMethods.Join.MakeGenericMethod(
                        typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
                    ServerQueryExpression,
                    innerQueryExpression.ServerQueryExpression,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector);
            }

            var entityShaper = new EntityShaperExpression(innerEntityProjection.EntityType, innerEntityProjection, nullable: innerNullable);
            entityProjectionExpression.AddNavigationBinding(navigation, entityShaper);

            return entityShaper;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine(nameof(InMemoryQueryExpression) + ": ");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine(nameof(ServerQueryExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(ServerQueryExpression);
                }

                expressionPrinter.AppendLine();
                expressionPrinter.AppendLine("ProjectionMapping:");
                using (expressionPrinter.Indent())
                {
                    foreach (var projectionMapping in _projectionMapping)
                    {
                        expressionPrinter.Append("Member: " + projectionMapping.Key + " Projection: ");
                        expressionPrinter.Visit(projectionMapping.Value);
                        expressionPrinter.AppendLine(",");
                    }
                }

                expressionPrinter.AppendLine();
            }
        }

        private Expression GetGroupingKey(Expression key, List<Expression> groupingExpressions, Expression groupingKeyAccessExpression)
        {
            switch (key)
            {
                case NewExpression newExpression:
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
                    return groupingKeyAccessExpression.CreateValueBufferReadValueExpression(
                        key.Type,
                        index,
                        InferPropertyFromInner(key));
            }
        }

        private void AddJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression? outerKeySelector,
            LambdaExpression? innerKeySelector,
            Type transparentIdentifierType,
            bool innerNullable)
        {
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(ValueBuffer), "inner");
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            var replacingVisitor = new ReplacingExpressionVisitor(
                new Expression[] { CurrentParameter, innerQueryExpression.CurrentParameter },
                new Expression[] { outerParameter, innerParameter });

            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Outer");
            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Inner");
            foreach (var projection in _projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        readExpressionMap[property] = CreateReadValueExpression(
                            replacedExpression.Type, GetIndex(replacedExpression), property);
                    }

                    projectionMapping[projection.Key.Prepend(outerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    projectionMapping[projection.Key.Prepend(outerMemberInfo)] = CreateReadValueExpression(
                        projection.Value.Type, GetIndex(replacedExpression), InferPropertyFromInner(projection.Value));
                }
            }

            var outerIndex = _projectionMappingExpressions.Count;
            foreach (var projection in innerQueryExpression._projectionMapping)
            {
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        var replacedExpression = replacingVisitor.Visit(entityProjection.BindProperty(property));
                        if (innerNullable)
                        {
                            replacedExpression = MakeReadValueNullable(replacedExpression);
                        }
                        readExpressionMap[property] = CreateReadValueExpression(
                            replacedExpression.Type, GetIndex(replacedExpression) + outerIndex, property);
                    }

                    projectionMapping[projection.Key.Prepend(innerMemberInfo)]
                        = new EntityProjectionExpression(entityProjection.EntityType, readExpressionMap);
                }
                else
                {
                    var replacedExpression = replacingVisitor.Visit(projection.Value);
                    if (innerNullable)
                    {
                        replacedExpression = MakeReadValueNullable(replacedExpression);
                    }
                    projectionMapping[projection.Key.Prepend(innerMemberInfo)] = CreateReadValueExpression(
                        replacedExpression.Type, GetIndex(replacedExpression) + outerIndex, InferPropertyFromInner(replacedExpression));
                }
            }

            var resultSelectorExpressions = new List<Expression>();
            foreach (var expression in _projectionMappingExpressions)
            {
                var updatedExpression = replacingVisitor.Visit(expression);
                resultSelectorExpressions.Add(
                    updatedExpression.Type.IsValueType ? Convert(updatedExpression, typeof(object)) : updatedExpression);
            }

            foreach (var expression in innerQueryExpression._projectionMappingExpressions)
            {
                var replacedExpression = replacingVisitor.Visit(expression);
                if (innerNullable)
                {
                    replacedExpression = MakeReadValueNullable(replacedExpression);
                }
                resultSelectorExpressions.Add(
                    replacedExpression.Type.IsValueType ? Convert(replacedExpression, typeof(object)) : replacedExpression);

                _projectionMappingExpressions.Add(
                    CreateReadValueExpression(
                        innerNullable ? expression.Type.MakeNullable() : expression.Type,
                        GetIndex(expression) + outerIndex,
                        InferPropertyFromInner(expression)));
            }

            var resultSelector = Lambda(
                New(_valueBufferConstructor, NewArrayInit(typeof(object), resultSelectorExpressions)),
                outerParameter,
                innerParameter);

            if (outerKeySelector != null
                && innerKeySelector != null)
            {
                if (innerNullable)
                {
                    ServerQueryExpression = Call(
                        _leftJoinMethodInfo.MakeGenericMethod(
                            typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
                        ServerQueryExpression,
                        innerQueryExpression.ServerQueryExpression,
                        outerKeySelector,
                        innerKeySelector,
                        resultSelector,
                        Constant(new ValueBuffer(
                            Enumerable.Repeat((object?)null, innerQueryExpression._projectionMappingExpressions.Count).ToArray())));
                }
                else
                {
                    ServerQueryExpression = Call(
                        EnumerableMethods.Join.MakeGenericMethod(
                            typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
                        ServerQueryExpression,
                        innerQueryExpression.ServerQueryExpression,
                        outerKeySelector,
                        innerKeySelector,
                        resultSelector);
                }
            }
            else
            {
                // inner nullable should do something different here
                // Issue#17536
                ServerQueryExpression = Call(
                    EnumerableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                        typeof(ValueBuffer), typeof(ValueBuffer), typeof(ValueBuffer)),
                    ServerQueryExpression,
                    Lambda(innerQueryExpression.ServerQueryExpression, CurrentParameter),
                    resultSelector);
            }

            _projectionMapping = projectionMapping;
        }

        private static int GetIndex(Expression expression)
                => (int)((ConstantExpression)((MethodCallExpression)expression).Arguments[1]).Value!;

        private MethodCallExpression CreateReadValueExpression(Type type, int index, IPropertyBase? property)
            => (MethodCallExpression)_valueBufferParameter.CreateValueBufferReadValueExpression(type, index, property);

        private IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredProperties());

        private static IPropertyBase? InferPropertyFromInner(Expression expression)
            => expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod
                    ? methodCallExpression.Arguments[2].GetConstantValue<IPropertyBase>()
                    : null;

        private static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            TInner defaultValue)
            => outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (oe, ies) => new { oe, ies })
                .SelectMany(t => t.ies.DefaultIfEmpty(defaultValue), (t, i) => resultSelector(t.oe, i));

        private MethodCallExpression MakeReadValueNullable(Expression expression)
        {
            Check.DebugAssert(expression is MethodCallExpression, "Expression must be method call expression.");

            var methodCallExpression = (MethodCallExpression)expression;

            return methodCallExpression.Type.IsNullableType()
                ? methodCallExpression
                : Call(
                    ExpressionExtensions.ValueBufferTryReadValueMethod.MakeGenericMethod(methodCallExpression.Type.MakeNullable()),
                    methodCallExpression.Arguments);
        }

        private sealed class ShaperRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly IDictionary<ProjectionMember, Expression> _projectionMapping;

            public ShaperRemappingExpressionVisitor(IDictionary<ProjectionMember, Expression> projectionMapping)
            {
                _projectionMapping = projectionMapping;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression
                    && projectionBindingExpression.ProjectionMember != null)
                {
                    var mappingValue = ((ConstantExpression)_projectionMapping[projectionBindingExpression.ProjectionMember]).Value;
                    return mappingValue is IReadOnlyDictionary<IProperty, int> indexMap
                        ? new ProjectionBindingExpression(projectionBindingExpression.QueryExpression, indexMap)
                        : mappingValue is int index
                            ? new ProjectionBindingExpression(
                                projectionBindingExpression.QueryExpression, index, projectionBindingExpression.Type)
                            : throw new InvalidOperationException(CoreStrings.UnknownEntity("ProjectionMapping"));
                }

                return base.Visit(expression);
            }
        }
    }
}
