// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        private static readonly ConstructorInfo _resultEnumerableConstructor
            = typeof(ResultEnumerable).GetConstructors().Single();

        private readonly ParameterExpression _valueBufferParameter;
        private ParameterExpression? _groupingParameter;
        private MethodInfo? _singleResultMethodInfo;
        private bool _scalarServerQuery;

        private Dictionary<ProjectionMember, Expression> _projectionMapping = new();
        private readonly List<Expression> _clientProjections = new();
        private readonly List<Expression> _projectionMappingExpressions = new();

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
            var propertyExpressionsMap = new Dictionary<IProperty, MethodCallExpression>();
            var selectorExpressions = new List<Expression>();
            foreach (var property in entityType.GetAllBaseTypesInclusive().SelectMany(et => et.GetDeclaredProperties()))
            {
                var propertyExpression = CreateReadValueExpression(property.ClrType, property.GetIndex(), property);
                selectorExpressions.Add(propertyExpression);

                Check.DebugAssert(property.GetIndex() == selectorExpressions.Count - 1,
                    "Properties should be ordered in same order as their indexes.");
                propertyExpressionsMap[property] = propertyExpression;
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
                                propertyExpressionsMap[discriminatorProperty],
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
                        propertyExpressionsMap[property] = readExpression;
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

            var entityProjection = new EntityProjectionExpression(entityType, propertyExpressionsMap);
            _projectionMapping[new ProjectionMember()] = entityProjection;
        }

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
        public virtual void ReplaceProjection(IReadOnlyList<Expression> clientProjections)
        {
            _projectionMapping.Clear();
            _projectionMappingExpressions.Clear();
            _clientProjections.Clear();
            _clientProjections.AddRange(clientProjections);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ReplaceProjection(IReadOnlyDictionary<ProjectionMember, Expression> projectionMapping)
        {
            _projectionMapping.Clear();
            _projectionMappingExpressions.Clear();
            _clientProjections.Clear();
            var selectorExpressions = new List<Expression>();
            foreach (var keyValuePair in projectionMapping)
            {
                if (keyValuePair.Value is EntityProjectionExpression entityProjectionExpression)
                {
                    _projectionMapping[keyValuePair.Key] = AddEntityProjection(entityProjectionExpression);
                }
                else
                {
                    selectorExpressions.Add(keyValuePair.Value);
                    var readExpression = CreateReadValueExpression(
                            keyValuePair.Value.Type, selectorExpressions.Count - 1, InferPropertyFromInner(keyValuePair.Value));
                    _projectionMapping[keyValuePair.Key] = readExpression;
                    _projectionMappingExpressions.Add(readExpression);
                }
            }

            if (selectorExpressions.Count == 0)
            {
                // No server correlated term in projection so add dummy 1.
                selectorExpressions.Add(Constant(1));
            }

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e).ToArray())),
                CurrentParameter);

            ServerQueryExpression = Call(
                EnumerableMethods.Select.MakeGenericMethod(CurrentParameter.Type, typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);

            _groupingParameter = null;

            EntityProjectionExpression AddEntityProjection(EntityProjectionExpression entityProjectionExpression)
            {
                var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
                foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
                {
                    var expression = entityProjectionExpression.BindProperty(property);
                    selectorExpressions.Add(expression);
                    var newExpression = CreateReadValueExpression(expression.Type, selectorExpressions.Count - 1, property);
                    readExpressionMap[property] = newExpression;
                    _projectionMappingExpressions.Add(newExpression);
                }

                var result = new EntityProjectionExpression(entityProjectionExpression.EntityType, readExpressionMap);

                // Also compute nested entity projections
                foreach (var navigation in entityProjectionExpression.EntityType.GetAllBaseTypes()
                    .Concat(entityProjectionExpression.EntityType.GetDerivedTypesInclusive())
                    .SelectMany(t => t.GetDeclaredNavigations()))
                {
                    var boundEntityShaperExpression = entityProjectionExpression.BindNavigation(navigation);
                    if (boundEntityShaperExpression != null)
                    {
                        var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                        var newInnerEntityProjection = AddEntityProjection(innerEntityProjection);
                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                        result.AddNavigationBinding(navigation, boundEntityShaperExpression);
                    }
                }

                return result;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember != null
                ? _projectionMapping[projectionBindingExpression.ProjectionMember]
                : _clientProjections[projectionBindingExpression.Index!.Value];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyProjection()
        {
            if (_scalarServerQuery)
            {
                _projectionMapping[new ProjectionMember()] = Constant(0);
                return;
            }

            var selectorExpressions = new List<Expression>();
            if (_clientProjections.Count > 0)
            {
                for (var i = 0; i < _clientProjections.Count; i++)
                {
                    var projection = _clientProjections[i];
                    switch (projection)
                    {
                        case EntityProjectionExpression entityProjectionExpression:
                        {
                            var indexMap = new Dictionary<IProperty, int>();
                            foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
                            {
                                selectorExpressions.Add(entityProjectionExpression.BindProperty(property));
                                indexMap[property] = selectorExpressions.Count - 1;
                            }

                            _clientProjections[i] = Constant(indexMap);
                            break;
                        }

                        case InMemoryQueryExpression inMemoryQueryExpression:
                        {
                            var singleResult = inMemoryQueryExpression._scalarServerQuery || inMemoryQueryExpression._singleResultMethodInfo != null;
                            inMemoryQueryExpression.ApplyProjection();
                            var serverQuery = inMemoryQueryExpression.ServerQueryExpression;
                            if (singleResult)
                            {
                                serverQuery = ((LambdaExpression)((NewExpression)serverQuery).Arguments[0]).Body;
                            }
                            selectorExpressions.Add(serverQuery);
                            _clientProjections[i] = Constant(selectorExpressions.Count - 1);
                            break;
                        }

                        default:
                            selectorExpressions.Add(projection);
                            _clientProjections[i] = Constant(selectorExpressions.Count - 1);
                            break;
                    }
                }
            }
            else
            {
                var newProjectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var keyValuePair in _projectionMapping)
                {
                    if (keyValuePair.Value is EntityProjectionExpression entityProjectionExpression)
                    {
                        var indexMap = new Dictionary<IProperty, int>();
                        foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
                        {
                            selectorExpressions.Add(entityProjectionExpression.BindProperty(property));
                            indexMap[property] = selectorExpressions.Count - 1;
                        }

                        newProjectionMapping[keyValuePair.Key] = Constant(indexMap);
                    }
                    else
                    {
                        selectorExpressions.Add(keyValuePair.Value);
                        newProjectionMapping[keyValuePair.Key] = Constant(selectorExpressions.Count - 1);
                    }
                }
                _projectionMapping = newProjectionMapping;
                _projectionMappingExpressions.Clear();
            }

            var selectorLambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e).ToArray())),
                CurrentParameter);

            ServerQueryExpression = Call(
                EnumerableMethods.Select.MakeGenericMethod(CurrentParameter.Type, typeof(ValueBuffer)),
                ServerQueryExpression,
                selectorLambda);

            if (_singleResultMethodInfo != null)
            {
                ServerQueryExpression = Call(
                    _singleResultMethodInfo.MakeGenericMethod(CurrentParameter.Type),
                    ServerQueryExpression);

                _singleResultMethodInfo = null;

                ConvertToEnumerable();
            }
        }

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
            if (_clientProjections.Count == 0)
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
                throw new InvalidOperationException(InMemoryStrings.SetOperationsNotAllowedAfterClientEvaluation);
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
            if (_clientProjections.Count != 0)
            {
                throw new InvalidOperationException(InMemoryStrings.DefaultIfEmptyAppliedAfterProjection);
            }

            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                projectionMapping[keyValuePair.Key] = keyValuePair.Value is EntityProjectionExpression entityProjectionExpression
                    ? MakeEntityProjectionNullable(entityProjectionExpression)
                    : (Expression)MakeReadValueNullable(keyValuePair.Value);
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
        public virtual void ApplyDistinct()
        {
            Check.DebugAssert(!_scalarServerQuery && _singleResultMethodInfo == null, "Cannot apply distinct on single result query");
            Check.DebugAssert(_groupingParameter == null, "Cannot apply distinct after GroupBy before flattening.");

            var selectorExpressions = new List<Expression>();
            if (_clientProjections.Count == 0)
            {
                selectorExpressions.AddRange(_projectionMappingExpressions);
                if (selectorExpressions.Count == 0)
                {
                    // No server correlated term in projection so add dummy 1.
                    selectorExpressions.Add(Constant(1));
                }
            }
            else
            {
                for (var i = 0; i < _clientProjections.Count; i++)
                {
                    var projection = _clientProjections[i];
                    if (projection is InMemoryQueryExpression)
                    {
                        throw new InvalidOperationException(InMemoryStrings.DistinctOnSubqueryNotSupported);
                    }

                    if (projection is EntityProjectionExpression entityProjectionExpression)
                    {
                        _clientProjections[i] = TraverseEntityProjection(selectorExpressions, entityProjectionExpression, makeNullable: false);
                    }
                    else
                    {
                        selectorExpressions.Add(projection);
                        _clientProjections[i] = CreateReadValueExpression(
                                projection.Type, selectorExpressions.Count - 1, InferPropertyFromInner(projection));

                    }
                }
            }

            var selectorLambda = Lambda(
                    New(
                        _valueBufferConstructor,
                        NewArrayInit(
                            typeof(object),
                            selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e).ToArray())),
                    CurrentParameter);

            ServerQueryExpression = Call(
                EnumerableMethods.Distinct.MakeGenericMethod(typeof(ValueBuffer)),
                Call(EnumerableMethods.Select.MakeGenericMethod(CurrentParameter.Type, typeof(ValueBuffer)),
                    ServerQueryExpression,
                    selectorLambda));
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
        public virtual Expression AddInnerJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            Expression outerShaperExpression,
            Expression innerShaperExpression)
            => AddJoin(innerQueryExpression, outerKeySelector, innerKeySelector, outerShaperExpression, innerShaperExpression, innerNullable: false);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression AddLeftJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            Expression outerShaperExpression,
            Expression innerShaperExpression)
            => AddJoin(innerQueryExpression, outerKeySelector, innerKeySelector, outerShaperExpression, innerShaperExpression, innerNullable: true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression AddSelectMany(
            InMemoryQueryExpression innerQueryExpression,
            Expression outerShaperExpression,
            Expression innerShaperExpression,
            bool innerNullable)
            => AddJoin(innerQueryExpression, null, null, outerShaperExpression, innerShaperExpression, innerNullable);

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
            Check.DebugAssert(_clientProjections.Count == 0, "Cannot expand weak entity navigation after client projection yet.");
            var innerNullable = !navigation.ForeignKey.IsRequiredDependent;
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(ValueBuffer), "inner");
            var replacingVisitor = new ReplacingExpressionVisitor(
                new Expression[] { CurrentParameter, innerQueryExpression.CurrentParameter },
                new Expression[] { outerParameter, innerParameter });

            var selectorExpressions = _projectionMappingExpressions.Select(e => replacingVisitor.Visit(e)).ToList();
            var outerIndex = selectorExpressions.Count;
            var innerEntityProjection = (EntityProjectionExpression)innerQueryExpression._projectionMapping[new ProjectionMember()];
            var innerReadExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(innerEntityProjection.EntityType))
            {
                var propertyExpression = innerEntityProjection.BindProperty(property);
                if (innerNullable)
                {
                    propertyExpression = MakeReadValueNullable(propertyExpression);
                }
                selectorExpressions.Add(propertyExpression);
                var readValueExperssion = CreateReadValueExpression(propertyExpression.Type, selectorExpressions.Count - 1, property);
                innerReadExpressionMap[property] = readValueExperssion;
                _projectionMappingExpressions.Add(readValueExperssion);
            }

            innerEntityProjection = new EntityProjectionExpression(innerEntityProjection.EntityType, innerReadExpressionMap);

            var resultSelector = Lambda(
                New(_valueBufferConstructor,
                    NewArrayInit(typeof(object),
                        selectorExpressions
                            .Select(e => replacingVisitor.Visit(e))
                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
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
                        Enumerable.Repeat((object?)null, selectorExpressions.Count - outerIndex).ToArray())));
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
        public virtual Expression GetSingleScalarProjection()
        {
            var expression = CreateReadValueExpression(ServerQueryExpression.Type, 0, null);
            _projectionMapping.Clear();
            _projectionMappingExpressions.Clear();
            _clientProjections.Clear();
            _projectionMapping[new ProjectionMember()] = expression;
            _projectionMappingExpressions.Add(expression);
            _groupingParameter = null;

            _scalarServerQuery = true;
            ConvertToEnumerable();

            return new ProjectionBindingExpression(this, new ProjectionMember(), expression.Type.MakeNullable());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ConvertToSingleResult(MethodInfo methodInfo)
        {
            _singleResultMethodInfo = methodInfo;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type => typeof(IEnumerable<ValueBuffer>);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

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
                if (_clientProjections.Count > 0)
                {
                    expressionPrinter.AppendLine("ClientProjections:");
                    using (expressionPrinter.Indent())
                    {
                        for (var i = 0; i < _clientProjections.Count; i++)
                        {
                            expressionPrinter.AppendLine();
                            expressionPrinter.Append(i.ToString()).Append(" -> ");
                            expressionPrinter.Visit(_clientProjections[i]);
                        }
                    }
                }
                else
                {
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

        private Expression AddJoin(
            InMemoryQueryExpression innerQueryExpression,
            LambdaExpression? outerKeySelector,
            LambdaExpression? innerKeySelector,
            Expression outerShaperExpression,
            Expression innerShaperExpression,
            bool innerNullable)
        {
            var transparentIdentifierType = TransparentIdentifierFactory.Create(outerShaperExpression.Type, innerShaperExpression.Type);
            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Outer");
            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Inner");
            var outerClientEval = _clientProjections.Count > 0;
            var innerClientEval = innerQueryExpression._clientProjections.Count > 0;
            var resultSelectorExpressions = new List<Expression>();
            var outerParameter = Parameter(typeof(ValueBuffer), "outer");
            var innerParameter = Parameter(typeof(ValueBuffer), "inner");
            var replacingVisitor = new ReplacingExpressionVisitor(
                new Expression[] { CurrentParameter, innerQueryExpression.CurrentParameter },
                new Expression[] { outerParameter, innerParameter });
            int outerIndex;

            if (outerClientEval)
            {
                // Outer projection are already populated
                if (innerClientEval)
                {
                    // Add inner to projection and update indexes
                    var indexMap = new int[innerQueryExpression._clientProjections.Count];
                    for (var i = 0; i < innerQueryExpression._clientProjections.Count; i++)
                    {
                        var projectionToAdd = innerQueryExpression._clientProjections[i];
                        projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                        _clientProjections.Add(projectionToAdd);
                        indexMap[i] = _clientProjections.Count - 1;
                    }
                    innerQueryExpression._clientProjections.Clear();

                    innerShaperExpression = new ProjectionIndexRemappingExpressionVisitor(innerQueryExpression, this, indexMap).Visit(innerShaperExpression);
                }
                else
                {
                    // Apply inner projection mapping and convert projection member binding to indexes
                    var mapping = ConvertProjectionMappingToClientProjections(innerQueryExpression._projectionMapping, innerNullable);
                    innerShaperExpression = new ProjectionMemberToIndexConvertingExpressionVisitor(this, mapping).Visit(innerShaperExpression);
                }

                // TODO: We still need to populate and generate result selector
                // Further for a subquery in projection we may need to update correlation terms used inside it.
                throw new NotImplementedException();
            }
            else
            {
                if (innerClientEval)
                {
                    // Since inner projections are populated, we need to populate outer also
                    var mapping = ConvertProjectionMappingToClientProjections(_projectionMapping);
                    outerShaperExpression = new ProjectionMemberToIndexConvertingExpressionVisitor(this, mapping).Visit(outerShaperExpression);

                    var indexMap = new int[innerQueryExpression._clientProjections.Count];
                    for (var i = 0; i < innerQueryExpression._clientProjections.Count; i++)
                    {
                        var projectionToAdd = innerQueryExpression._clientProjections[i];
                        projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                        _clientProjections.Add(projectionToAdd);
                        indexMap[i] = _clientProjections.Count - 1;
                    }
                    innerQueryExpression._clientProjections.Clear();

                    innerShaperExpression = new ProjectionIndexRemappingExpressionVisitor(innerQueryExpression, this, indexMap).Visit(innerShaperExpression);
                    // TODO: We still need to populate and generate result selector
                    // Further for a subquery in projection we may need to update correlation terms used inside it.
                    throw new NotImplementedException();
                }
                else
                {
                    var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                    var mapping = new Dictionary<ProjectionMember, ProjectionMember>();
                    foreach (var projection in _projectionMapping)
                    {
                        var newProjectionMember = projection.Key.Prepend(outerMemberInfo);
                        mapping[projection.Key] = newProjectionMember;
                        if (projection.Value is EntityProjectionExpression entityProjectionExpression)
                        {
                            projectionMapping[newProjectionMember] = TraverseEntityProjection(
                                resultSelectorExpressions, entityProjectionExpression, makeNullable: false);
                        }
                        else
                        {
                            resultSelectorExpressions.Add(projection.Value);
                            projectionMapping[newProjectionMember] = CreateReadValueExpression(
                                projection.Value.Type, resultSelectorExpressions.Count - 1, InferPropertyFromInner(projection.Value));
                        }
                    }
                    outerShaperExpression = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(outerShaperExpression);
                    mapping.Clear();

                    outerIndex = resultSelectorExpressions.Count;
                    foreach (var projection in innerQueryExpression._projectionMapping)
                    {
                        var newProjectionMember = projection.Key.Prepend(innerMemberInfo);
                        mapping[projection.Key] = newProjectionMember;
                        if (projection.Value is EntityProjectionExpression entityProjectionExpression)
                        {
                            projectionMapping[newProjectionMember] = TraverseEntityProjection(
                                resultSelectorExpressions, entityProjectionExpression, innerNullable);
                        }
                        else
                        {
                            var expression = projection.Value;
                            if (innerNullable)
                            {
                                expression = MakeReadValueNullable(expression);
                            }
                            resultSelectorExpressions.Add(expression);
                            projectionMapping[newProjectionMember] = CreateReadValueExpression(
                                expression.Type, resultSelectorExpressions.Count - 1, InferPropertyFromInner(projection.Value));
                        }
                    }
                    innerShaperExpression = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(innerShaperExpression);
                    mapping.Clear();

                    _projectionMapping = projectionMapping;
                }
            }

            var resultSelector = Lambda(
                New(
                    _valueBufferConstructor, NewArrayInit(typeof(object),
                    resultSelectorExpressions.Select((e, i) =>
                    {
                        var expression = replacingVisitor.Visit(e);
                        if (innerNullable
                            && i > outerIndex)
                        {
                            expression = MakeReadValueNullable(expression);
                        }

                        if (expression.Type.IsValueType)
                        {
                            expression = Convert(expression, typeof(object));
                        }

                        return expression;
                    }))),
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
                            Enumerable.Repeat((object?)null, resultSelectorExpressions.Count - outerIndex).ToArray())));
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

            if (innerNullable)
            {
                innerShaperExpression = new EntityShaperNullableMarkingExpressionVisitor().Visit(innerShaperExpression);
            }

            return New(
                transparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
                new[] { outerShaperExpression, innerShaperExpression }, outerMemberInfo, innerMemberInfo);

            static Expression MakeNullable(Expression expression, bool nullable)
                => nullable
                    ? expression is EntityProjectionExpression entityProjection
                        ? MakeEntityProjectionNullable(entityProjection)
                        : MakeReadValueNullable(expression)
                    : expression;
        }

        private void ConvertToEnumerable()
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
                        _resultEnumerableConstructor,
                        Lambda<Func<ValueBuffer>>(
                            New(
                                _valueBufferConstructor,
                                NewArrayInit(typeof(object), ServerQueryExpression))));
                }
                else
                {
                    ServerQueryExpression = New(
                        _resultEnumerableConstructor,
                        Lambda<Func<ValueBuffer>>(ServerQueryExpression));
                }
            }
        }

        private MethodCallExpression CreateReadValueExpression(Type type, int index, IPropertyBase? property)
            => (MethodCallExpression)_valueBufferParameter.CreateValueBufferReadValueExpression(type, index, property);

        private static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredProperties());

        private static IPropertyBase? InferPropertyFromInner(Expression expression)
            => expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod
                    ? methodCallExpression.Arguments[2].GetConstantValue<IPropertyBase>()
                    : null;

        private static EntityProjectionExpression MakeEntityProjectionNullable(EntityProjectionExpression entityProjectionExpression)
        {
            var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
            {
                readExpressionMap[property] = MakeReadValueNullable(entityProjectionExpression.BindProperty(property));
            }

            var result = new EntityProjectionExpression(entityProjectionExpression.EntityType, readExpressionMap);

            // Also compute nested entity projections
            foreach (var navigation in entityProjectionExpression.EntityType.GetAllBaseTypes()
                .Concat(entityProjectionExpression.EntityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredNavigations()))
            {
                var boundEntityShaperExpression = entityProjectionExpression.BindNavigation(navigation);
                if (boundEntityShaperExpression != null)
                {
                    var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                    var newInnerEntityProjection = MakeEntityProjectionNullable(innerEntityProjection);
                    boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                    result.AddNavigationBinding(navigation, boundEntityShaperExpression);
                }
            }

            return result;
        }

        private Dictionary<ProjectionMember, int> ConvertProjectionMappingToClientProjections(
            Dictionary<ProjectionMember, Expression> projectionMapping,
            bool makeNullable = false)
        {
            var mapping = new Dictionary<ProjectionMember, int>();
            var entityProjectionCache = new Dictionary<EntityProjectionExpression, int>(ReferenceEqualityComparer.Instance);
            foreach (var projection in projectionMapping)
            {
                var projectionMember = projection.Key;
                var projectionToAdd = projection.Value;

                if (projectionToAdd is EntityProjectionExpression entityProjection)
                {
                    if (!entityProjectionCache.TryGetValue(entityProjection, out var value))
                    {
                        var entityProjectionToCache = entityProjection;
                        if (makeNullable)
                        {
                            entityProjection = MakeEntityProjectionNullable(entityProjection);
                        }
                        _clientProjections.Add(entityProjection);
                        value = _clientProjections.Count - 1;
                        entityProjectionCache[entityProjectionToCache] = value;
                    }

                    mapping[projectionMember] = value;
                }
                else
                {
                    if (makeNullable)
                    {
                        projectionToAdd = MakeReadValueNullable(projectionToAdd);
                    }
                    var existingIndex = _clientProjections.FindIndex(e => e.Equals(projectionToAdd));
                    if (existingIndex == -1)
                    {
                        _clientProjections.Add(projectionToAdd);
                        existingIndex = _clientProjections.Count - 1;
                    }
                    mapping[projectionMember] = existingIndex;
                }
            }

            projectionMapping.Clear();

            return mapping;
        }

        private static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector,
            TInner defaultValue)
            => outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (oe, ies) => new { oe, ies })
                .SelectMany(t => t.ies.DefaultIfEmpty(defaultValue), (t, i) => resultSelector(t.oe, i));

        private static MethodCallExpression MakeReadValueNullable(Expression expression)
        {
            Check.DebugAssert(expression is MethodCallExpression, "Expression must be method call expression.");

            var methodCallExpression = (MethodCallExpression)expression;

            return methodCallExpression.Type.IsNullableType()
                ? methodCallExpression
                : Call(
                    ExpressionExtensions.ValueBufferTryReadValueMethod.MakeGenericMethod(methodCallExpression.Type.MakeNullable()),
                    methodCallExpression.Arguments);
        }

        private EntityProjectionExpression TraverseEntityProjection(
            List<Expression> selectorExpressions, EntityProjectionExpression entityProjectionExpression, bool makeNullable)
        {
            var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
            {
                var expression = entityProjectionExpression.BindProperty(property);
                if (makeNullable)
                {
                    expression = MakeReadValueNullable(expression);
                }
                selectorExpressions.Add(expression);
                var newExpression = CreateReadValueExpression(expression.Type, selectorExpressions.Count - 1, property);
                readExpressionMap[property] = newExpression;
            }

            var result = new EntityProjectionExpression(entityProjectionExpression.EntityType, readExpressionMap);

            // Also compute nested entity projections
            foreach (var navigation in entityProjectionExpression.EntityType.GetAllBaseTypes()
                .Concat(entityProjectionExpression.EntityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredNavigations()))
            {
                var boundEntityShaperExpression = entityProjectionExpression.BindNavigation(navigation);
                if (boundEntityShaperExpression != null)
                {
                    var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                    var newInnerEntityProjection = TraverseEntityProjection(selectorExpressions, innerEntityProjection, makeNullable);
                    boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                    result.AddNavigationBinding(navigation, boundEntityShaperExpression);
                }
            }

            return result;
        }
    }
}
