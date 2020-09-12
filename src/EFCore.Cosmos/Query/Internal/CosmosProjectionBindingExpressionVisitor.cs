// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(CosmosProjectionBindingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

        private readonly CosmosSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly IModel _model;
        private SelectExpression _selectExpression;
        private bool _clientEval;

        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();

        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();

        private readonly IDictionary<ParameterExpression, CollectionShaperExpression> _collectionShaperMapping
            = new Dictionary<ParameterExpression, CollectionShaperExpression>();

        private readonly Stack<INavigation> _includedNavigations
            = new Stack<INavigation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosProjectionBindingExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] CosmosSqlTranslatingExpressionVisitor sqlTranslator)
        {
            _model = model;
            _sqlTranslator = sqlTranslator;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Translate([NotNull] SelectExpression selectExpression, [NotNull] Expression expression)
        {
            _selectExpression = selectExpression;
            _clientEval = false;

            _projectionMembers.Push(new ProjectionMember());

            var result = Visit(expression);
            if (result == null)
            {
                _clientEval = true;

                result = Visit(expression);

                _projectionMapping.Clear();
            }

            _selectExpression.ReplaceProjectionMapping(_projectionMapping);
            _selectExpression = null;
            _projectionMembers.Clear();
            _projectionMapping.Clear();

            result = MatchTypes(result, expression.Type);

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression is NewExpression
                || expression is MemberInitExpression
                || expression is EntityShaperExpression)
            {
                return base.Visit(expression);
            }

            // This skips the group parameter from GroupJoin
            if (expression is ParameterExpression parameter
                && parameter.Type.IsGenericType
                && parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return parameter;
            }

            if (_clientEval)
            {
                switch (expression)
                {
                    case ConstantExpression _:
                        return expression;

                    case ParameterExpression parameterExpression:
                        if (_collectionShaperMapping.ContainsKey(parameterExpression))
                        {
                            return parameterExpression;
                        }

                        if (parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal)
                            == true)
                        {
                            return Expression.Call(
                                _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                                QueryCompilationContext.QueryContextParameter,
                                Expression.Constant(parameterExpression.Name));
                        }

                        throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));

                    case MaterializeCollectionNavigationExpression _:
                        return base.Visit(expression);
                }

                var translation = _sqlTranslator.Translate(expression);
                if (translation == null)
                {
                    return base.Visit(expression);
                }

                return new ProjectionBindingExpression(
                    _selectExpression, _selectExpression.AddToProjection(translation), expression.Type.MakeNullable());
            }
            else
            {
                var translation = _sqlTranslator.Translate(expression);
                if (translation == null)
                {
                    return null;
                }

                _projectionMapping[_projectionMembers.Peek()] = translation;

                return new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), expression.Type.MakeNullable());
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var left = MatchTypes(Visit(binaryExpression.Left), binaryExpression.Left.Type);
            var right = MatchTypes(Visit(binaryExpression.Right), binaryExpression.Right.Type);

            return binaryExpression.Update(left, VisitAndConvert(binaryExpression.Conversion, "VisitBinary"), right);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            if (test.Type == typeof(bool?))
            {
                test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
            }

            return conditionalExpression.Update(test, ifTrue, ifFalse);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case EntityShaperExpression entityShaperExpression:
                {
                    var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                    VerifySelectExpression(projectionBindingExpression);

                    if (_clientEval)
                    {
                        var entityProjection = (EntityProjectionExpression)_selectExpression.GetMappedProjection(
                            projectionBindingExpression.ProjectionMember);

                        return entityShaperExpression.Update(
                            new ProjectionBindingExpression(
                                _selectExpression, _selectExpression.AddToProjection(entityProjection), typeof(ValueBuffer)));
                    }

                    _projectionMapping[_projectionMembers.Peek()]
                        = _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);

                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
                }

                case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                    return materializeCollectionNavigationExpression.Navigation is INavigation embeddableNavigation
                        && embeddableNavigation.IsEmbedded()
                            ? base.Visit(materializeCollectionNavigationExpression.Subquery)
                            : base.VisitExtension(materializeCollectionNavigationExpression);

                case IncludeExpression includeExpression:
                    if (!_clientEval)
                    {
                        return null;
                    }

                    if (!(includeExpression.Navigation is INavigation includableNavigation
                        && includableNavigation.IsEmbedded()))
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NonEmbeddedIncludeNotSupported(includeExpression.Navigation));
                    }

                    _includedNavigations.Push(includableNavigation);

                    var newIncludeExpression = base.VisitExtension(includeExpression);

                    _includedNavigations.Pop();

                    return newIncludeExpression;

                default:
                    throw new InvalidOperationException(CoreStrings.QueryFailed(extensionExpression.Print(), GetType().Name));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override ElementInit VisitElementInit(ElementInit elementInit)
            => elementInit.Update(elementInit.Arguments.Select(e => MatchTypes(Visit(e), e.Type)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            if (!_clientEval)
            {
                return null;
            }

            var innerExpression = Visit(memberExpression.Expression);

            EntityShaperExpression shaperExpression;
            switch (innerExpression)
            {
                case EntityShaperExpression shaper:
                    shaperExpression = shaper;
                    break;

                case UnaryExpression unaryExpression:
                    shaperExpression = unaryExpression.Operand as EntityShaperExpression;
                    if (shaperExpression == null
                        || unaryExpression.NodeType != ExpressionType.Convert)
                    {
                        return NullSafeUpdate(innerExpression);
                    }

                    break;

                default:
                    return NullSafeUpdate(innerExpression);
            }

            EntityProjectionExpression innerEntityProjection;
            switch (shaperExpression.ValueBufferExpression)
            {
                case ProjectionBindingExpression innerProjectionBindingExpression:
                    innerEntityProjection = (EntityProjectionExpression)_selectExpression.Projection[
                        innerProjectionBindingExpression.Index.Value].Expression;
                    break;

                case UnaryExpression unaryExpression:
                    // Unwrap EntityProjectionExpression when the root entity is not projected
                    innerEntityProjection = (EntityProjectionExpression)((UnaryExpression)unaryExpression.Operand).Operand;
                    break;

                default:
                    throw new InvalidOperationException(CoreStrings.QueryFailed(memberExpression.Print(), GetType().Name));
            }

            var navigationProjection = innerEntityProjection.BindMember(
                memberExpression.Member, innerExpression.Type, clientEval: true, out var propertyBase);

            if (!(propertyBase is INavigation navigation)
                || !navigation.IsEmbedded())
            {
                return NullSafeUpdate(innerExpression);
            }

            switch (navigationProjection)
            {
                case EntityProjectionExpression entityProjection:
                    return new EntityShaperExpression(
                        navigation.TargetEntityType,
                        Expression.Convert(Expression.Convert(entityProjection, typeof(object)), typeof(ValueBuffer)),
                        nullable: true);

                case ObjectArrayProjectionExpression objectArrayProjectionExpression:
                {
                    var innerShaperExpression = new EntityShaperExpression(
                        navigation.TargetEntityType,
                        Expression.Convert(
                            Expression.Convert(objectArrayProjectionExpression.InnerProjection, typeof(object)), typeof(ValueBuffer)),
                        nullable: true);

                    return new CollectionShaperExpression(
                        objectArrayProjectionExpression,
                        innerShaperExpression,
                        navigation,
                        innerShaperExpression.EntityType.ClrType);
                }

                default:
                    throw new InvalidOperationException(CoreStrings.QueryFailed(memberExpression.Print(), GetType().Name));
            }

            Expression NullSafeUpdate(Expression expression)
            {
                Expression updatedMemberExpression = memberExpression.Update(
                    expression != null ? MatchTypes(expression, memberExpression.Expression.Type) : expression);

                if (expression?.Type.IsNullableType() == true)
                {
                    var nullableReturnType = memberExpression.Type.MakeNullable();
                    if (!memberExpression.Type.IsNullableType())
                    {
                        updatedMemberExpression = Expression.Convert(updatedMemberExpression, nullableReturnType);
                    }

                    updatedMemberExpression = Expression.Condition(
                        Expression.Equal(expression, Expression.Default(expression.Type)),
                        Expression.Constant(null, nullableReturnType),
                        updatedMemberExpression);
                }

                return updatedMemberExpression;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
        {
            var expression = memberAssignment.Expression;
            Expression visitedExpression;
            if (_clientEval)
            {
                visitedExpression = Visit(memberAssignment.Expression);
            }
            else
            {
                var projectionMember = _projectionMembers.Peek().Append(memberAssignment.Member);
                _projectionMembers.Push(projectionMember);

                visitedExpression = Visit(memberAssignment.Expression);
                if (visitedExpression == null)
                {
                    return null;
                }

                _projectionMembers.Pop();
            }

            visitedExpression = MatchTypes(visitedExpression, expression.Type);

            return memberAssignment.Update(visitedExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            Check.NotNull(memberInitExpression, nameof(memberInitExpression));

            var newExpression = Visit(memberInitExpression.NewExpression);
            if (newExpression == null)
            {
                return null;
            }

            var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
            for (var i = 0; i < newBindings.Length; i++)
            {
                if (memberInitExpression.Bindings[i].BindingType != MemberBindingType.Assignment)
                {
                    return null;
                }

                newBindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);

                if (newBindings[i] == null)
                {
                    return null;
                }
            }

            return memberInitExpression.Update((NewExpression)newExpression, newBindings);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var memberName)
                || methodCallExpression.TryGetIndexerArguments(_model, out source, out memberName))
            {
                if (!_clientEval)
                {
                    return null;
                }

                var visitedSource = Visit(source);

                EntityShaperExpression shaperExpression;
                switch (visitedSource)
                {
                    case EntityShaperExpression shaper:
                        shaperExpression = shaper;
                        break;

                    case UnaryExpression unaryExpression:
                        shaperExpression = unaryExpression.Operand as EntityShaperExpression;
                        if (shaperExpression == null
                            || unaryExpression.NodeType != ExpressionType.Convert)
                        {
                            return null;
                        }

                        break;

                    case ParameterExpression parameterExpression:
                        if (!_collectionShaperMapping.TryGetValue(parameterExpression, out var collectionShaper))
                        {
                            return null;
                        }

                        shaperExpression = (EntityShaperExpression)collectionShaper.InnerShaper;
                        break;

                    default:
                        return null;
                }

                EntityProjectionExpression innerEntityProjection;
                switch (shaperExpression.ValueBufferExpression)
                {
                    case ProjectionBindingExpression innerProjectionBindingExpression:
                        innerEntityProjection = (EntityProjectionExpression)_selectExpression.Projection[
                            innerProjectionBindingExpression.Index.Value].Expression;
                        break;

                    case UnaryExpression unaryExpression:
                        innerEntityProjection = (EntityProjectionExpression)((UnaryExpression)unaryExpression.Operand).Operand;
                        break;

                    default:
                        throw new InvalidOperationException(CoreStrings.QueryFailed(methodCallExpression.Print(), GetType().Name));
                }

                Expression navigationProjection;
                var navigation = _includedNavigations.FirstOrDefault(n => n.Name == memberName);
                if (navigation == null)
                {
                    navigationProjection = innerEntityProjection.BindMember(
                        memberName, visitedSource.Type, clientEval: true, out var propertyBase);

                    if (!(propertyBase is INavigation projectedNavigation)
                        || !projectedNavigation.IsEmbedded())
                    {
                        return null;
                    }

                    navigation = projectedNavigation;
                }
                else
                {
                    navigationProjection = innerEntityProjection.BindNavigation(navigation, clientEval: true);
                }

                switch (navigationProjection)
                {
                    case EntityProjectionExpression entityProjection:
                        return new EntityShaperExpression(
                            navigation.TargetEntityType,
                            Expression.Convert(Expression.Convert(entityProjection, typeof(object)), typeof(ValueBuffer)),
                            nullable: true);

                    case ObjectArrayProjectionExpression objectArrayProjectionExpression:
                    {
                        var innerShaperExpression = new EntityShaperExpression(
                            navigation.TargetEntityType,
                            Expression.Convert(
                                Expression.Convert(objectArrayProjectionExpression.InnerProjection, typeof(object)), typeof(ValueBuffer)),
                            nullable: true);

                        return new CollectionShaperExpression(
                            objectArrayProjectionExpression,
                            innerShaperExpression,
                            navigation,
                            innerShaperExpression.EntityType.ClrType);
                    }

                    default:
                        throw new InvalidOperationException(CoreStrings.QueryFailed(methodCallExpression.Print(), GetType().Name));
                }
            }

            if (_clientEval)
            {
                var method = methodCallExpression.Method;
                if (method.DeclaringType == typeof(Queryable))
                {
                    var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                    var visitedSource = Visit(methodCallExpression.Arguments[0]);

                    switch (method.Name)
                    {
                        case nameof(Queryable.AsQueryable)
                            when genericMethod == QueryableMethods.AsQueryable:
                            // Unwrap AsQueryable
                            return visitedSource;

                        case nameof(Queryable.Select)
                            when genericMethod == QueryableMethods.Select:
                            if (!(visitedSource is CollectionShaperExpression shaper))
                            {
                                return null;
                            }

                            var lambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

                            _collectionShaperMapping.Add(lambda.Parameters.Single(), shaper);

                            lambda = Expression.Lambda(Visit(lambda.Body), lambda.Parameters);
                            return Expression.Call(
                                EnumerableMethods.Select.MakeGenericMethod(method.GetGenericArguments()),
                                shaper,
                                lambda);
                    }
                }
            }

            var @object = Visit(methodCallExpression.Object);
            var arguments = new Expression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
            {
                var argument = methodCallExpression.Arguments[i];
                arguments[i] = MatchTypes(Visit(argument), argument.Type);
            }

            Expression updatedMethodCallExpression = methodCallExpression.Update(
                @object != null ? MatchTypes(@object, methodCallExpression.Object.Type) : @object,
                arguments);

            if (@object?.Type.IsNullableType() == true
                && !methodCallExpression.Object.Type.IsNullableType())
            {
                var nullableReturnType = methodCallExpression.Type.MakeNullable();
                if (!methodCallExpression.Type.IsNullableType())
                {
                    updatedMethodCallExpression = Expression.Convert(updatedMethodCallExpression, nullableReturnType);
                }

                return Expression.Condition(
                    Expression.Equal(@object, Expression.Default(@object.Type)),
                    Expression.Constant(null, nullableReturnType),
                    updatedMethodCallExpression);
            }

            return updatedMethodCallExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNew(NewExpression newExpression)
        {
            Check.NotNull(newExpression, nameof(newExpression));

            if (newExpression.Arguments.Count == 0)
            {
                return newExpression;
            }

            if (!_clientEval
                && newExpression.Members == null)
            {
                return null;
            }

            var newArguments = new Expression[newExpression.Arguments.Count];
            for (var i = 0; i < newArguments.Length; i++)
            {
                var argument = newExpression.Arguments[i];
                Expression visitedArgument;
                if (_clientEval)
                {
                    visitedArgument = Visit(argument);
                }
                else
                {
                    var projectionMember = _projectionMembers.Peek().Append(newExpression.Members[i]);
                    _projectionMembers.Push(projectionMember);
                    visitedArgument = Visit(argument);
                    if (visitedArgument == null)
                    {
                        return null;
                    }

                    _projectionMembers.Pop();
                }

                newArguments[i] = MatchTypes(visitedArgument, argument.Type);
            }

            return newExpression.Update(newArguments);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
            => newArrayExpression.Update(newArrayExpression.Expressions.Select(e => MatchTypes(Visit(e), e.Type)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);

            return (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                && unaryExpression.Type == operand.Type
                    ? operand
                    : unaryExpression.Update(MatchTypes(operand, unaryExpression.Operand.Type));
        }

        // TODO: Debugging
        private void VerifySelectExpression(ProjectionBindingExpression projectionBindingExpression)
        {
            if (projectionBindingExpression.QueryExpression != _selectExpression)
            {
                throw new InvalidOperationException(CoreStrings.QueryFailed(projectionBindingExpression.Print(), GetType().Name));
            }
        }

        private static Expression MatchTypes(Expression expression, Type targetType)
        {
            if (targetType != expression.Type
                && targetType.TryGetSequenceType() == null)
            {
                Check.DebugAssert(targetType.MakeNullable() == expression.Type, "expression.Type must be nullable of targetType");

                expression = Expression.Convert(expression, targetType);
            }

            return expression;
        }

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
            => (T)queryContext.ParameterValues[parameterName];
    }
}
