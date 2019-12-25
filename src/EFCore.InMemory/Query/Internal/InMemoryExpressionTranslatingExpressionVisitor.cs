// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryExpressionTranslatingExpressionVisitor : ExpressionVisitor
    {
        private const string CompiledQueryParameterPrefix = "__";

        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly EntityProjectionFindingExpressionVisitor _entityProjectionFindingExpressionVisitor;
        private readonly IModel _model;

        public InMemoryExpressionTranslatingExpressionVisitor(
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            [NotNull] IModel model)
        {
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _entityProjectionFindingExpressionVisitor = new EntityProjectionFindingExpressionVisitor();
            _model = model;
        }

        private sealed class EntityProjectionFindingExpressionVisitor : ExpressionVisitor
        {
            private bool _found;

            public bool Find(Expression expression)
            {
                _found = false;

                Visit(expression);

                return _found;
            }

            public override Expression Visit(Expression expression)
            {
                if (_found)
                {
                    return expression;
                }

                if (expression is EntityProjectionExpression)
                {
                    _found = true;
                    return expression;
                }

                return base.Visit(expression);
            }
        }

        private sealed class PropertyFindingExpressionVisitor : ExpressionVisitor
        {
            private readonly IModel _model;
            private IProperty _property;

            public PropertyFindingExpressionVisitor(IModel model)
            {
                _model = model;
            }

            public IProperty Find(Expression expression)
            {
                Visit(expression);

                return _property;
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                var entityType = FindEntityType(memberExpression.Expression);
                if (entityType != null)
                {
                    _property = GetProperty(entityType, MemberIdentity.Create(memberExpression.Member));
                }

                return memberExpression;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName)
                    || methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
                {
                    var entityType = FindEntityType(source);
                    if (entityType != null)
                    {
                        _property = GetProperty(entityType, MemberIdentity.Create(propertyName));
                    }
                }

                return methodCallExpression;
            }

            private static IProperty GetProperty(IEntityType entityType, MemberIdentity memberIdentity)
                => memberIdentity.MemberInfo != null
                    ? entityType.FindProperty(memberIdentity.MemberInfo)
                    : entityType.FindProperty(memberIdentity.Name);

            private static IEntityType FindEntityType(Expression source)
            {
                source = source.UnwrapTypeConversion(out var convertedType);

                if (source is EntityShaperExpression entityShaperExpression)
                {
                    var entityType = entityShaperExpression.EntityType;
                    if (convertedType != null)
                    {
                        entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                            .FirstOrDefault(et => et.ClrType == convertedType);
                    }

                    return entityType;
                }

                return null;
            }
        }

        public virtual Expression Translate([NotNull] Expression expression)
        {
            var result = Visit(expression);

            return _entityProjectionFindingExpressionVisitor.Find(result)
                ? null
                : result;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

            if (newLeft == null
                || newRight == null)
            {
                return null;
            }

            if (IsConvertedToNullable(newLeft, binaryExpression.Left)
                || IsConvertedToNullable(newRight, binaryExpression.Right))
            {
                newLeft = ConvertToNullable(newLeft);
                newRight = ConvertToNullable(newRight);
            }

            var propertyFindingExpressionVisitor = new PropertyFindingExpressionVisitor(_model);
            var property = propertyFindingExpressionVisitor.Find(binaryExpression.Left)
                ?? propertyFindingExpressionVisitor.Find(binaryExpression.Right);

            if (property != null)
            {
                var comparer = property.GetValueComparer();

                if (comparer != null
                    && comparer.Type.IsAssignableFrom(newLeft.Type)
                    && comparer.Type.IsAssignableFrom(newRight.Type))
                {
                    if (binaryExpression.NodeType == ExpressionType.Equal)
                    {
                        return comparer.ExtractEqualsBody(newLeft, newRight);
                    }

                    if (binaryExpression.NodeType == ExpressionType.NotEqual)
                    {
                        return Expression.IsFalse(comparer.ExtractEqualsBody(newLeft, newRight));
                    }
                }
            }

            return Expression.MakeBinary(
                binaryExpression.NodeType,
                newLeft,
                newRight,
                binaryExpression.IsLiftedToNull,
                binaryExpression.Method,
                binaryExpression.Conversion);
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            if (test == null
                || ifTrue == null
                || ifFalse == null)
            {
                return null;
            }

            if (test.Type == typeof(bool?))
            {
                test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
            }

            if (IsConvertedToNullable(ifTrue, conditionalExpression.IfTrue)
                || IsConvertedToNullable(ifFalse, conditionalExpression.IfFalse))
            {
                ifTrue = ConvertToNullable(ifTrue);
                ifFalse = ConvertToNullable(ifFalse);
            }

            return Expression.Condition(test, ifTrue, ifFalse);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            if (TryBindMember(
                memberExpression.Expression,
                MemberIdentity.Create(memberExpression.Member),
                memberExpression.Type,
                out var result))
            {
                return result;
            }

            var innerExpression = Visit(memberExpression.Expression);
            if (memberExpression.Expression != null
                && innerExpression == null)
            {
                return null;
            }

            var updatedMemberExpression = (Expression)memberExpression.Update(innerExpression);
            if (innerExpression != null
                && innerExpression.Type.IsNullableType()
                && ShouldApplyNullProtectionForMemberAccess(innerExpression.Type, memberExpression.Member.Name))
            {
                updatedMemberExpression = ConvertToNullable(updatedMemberExpression);

                return Expression.Condition(
                    Expression.Equal(innerExpression, Expression.Default(innerExpression.Type)),
                    Expression.Default(updatedMemberExpression.Type),
                    updatedMemberExpression);
            }

            return updatedMemberExpression;

            static bool ShouldApplyNullProtectionForMemberAccess(Type callerType, string memberName)
                => !(callerType.IsGenericType
                    && callerType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && (memberName == nameof(Nullable<int>.Value) || memberName == nameof(Nullable<int>.HasValue)));
        }

        private bool TryBindMember(Expression source, MemberIdentity memberIdentity, Type type, out Expression result)
        {
            source = source.UnwrapTypeConversion(out var convertedType);
            result = null;
            if (source is EntityShaperExpression entityShaperExpression)
            {
                var entityType = entityShaperExpression.EntityType;
                if (convertedType != null)
                {
                    entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                        .FirstOrDefault(et => et.ClrType == convertedType);
                    if (entityType == null)
                    {
                        return false;
                    }
                }

                var property = memberIdentity.MemberInfo != null
                    ? entityType.FindProperty(memberIdentity.MemberInfo)
                    : entityType.FindProperty(memberIdentity.Name);
                if (property != null
                    && Visit(entityShaperExpression.ValueBufferExpression) is EntityProjectionExpression entityProjectionExpression
                    && (entityProjectionExpression.EntityType.IsAssignableFrom(property.DeclaringEntityType)
                        || property.DeclaringEntityType.IsAssignableFrom(entityProjectionExpression.EntityType)))
                {
                    result = BindProperty(entityProjectionExpression, property);

                    // if the result type change was just nullability change e.g from int to int?
                    // we want to preserve the new type for null propagation
                    if (result.Type != type
                        && !(result.Type.IsNullableType()
                            && !type.IsNullableType()
                            && result.Type.UnwrapNullableType() == type))
                    {
                        result = Expression.Convert(result, type);
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool IsConvertedToNullable(Expression result, Expression original)
            => result.Type.IsNullableType()
                && !original.Type.IsNullableType()
                && result.Type.UnwrapNullableType() == original.Type;

        private static Expression ConvertToNullable(Expression expression)
            => !expression.Type.IsNullableType()
                ? Expression.Convert(expression, expression.Type.MakeNullable())
                : expression;

        private static Expression ConvertToNonNullable(Expression expression)
            => expression.Type.IsNullableType()
                ? Expression.Convert(expression, expression.Type.UnwrapNullableType())
                : expression;

        private static Expression BindProperty(EntityProjectionExpression entityProjectionExpression, IProperty property)
            => entityProjectionExpression.BindProperty(property);

        private static Expression GetSelector(MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
        {
            if (methodCallExpression.Arguments.Count == 1)
            {
                return groupByShaperExpression.ElementSelector;
            }

            if (methodCallExpression.Arguments.Count == 2)
            {
                var selectorLambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                return ReplacingExpressionVisitor.Replace(
                    selectorLambda.Parameters[0],
                    groupByShaperExpression.ElementSelector,
                    selectorLambda.Body);
            }

            throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
        }

        private Expression GetPredicate(MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
        {
            if (methodCallExpression.Arguments.Count == 1)
            {
                return null;
            }

            if (methodCallExpression.Arguments.Count == 2)
            {
                var selectorLambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                return ReplacingExpressionVisitor.Replace(
                    selectorLambda.Parameters[0],
                    groupByShaperExpression.ElementSelector,
                    selectorLambda.Body);
            }

            throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
            {
                return methodCallExpression;
            }

            // EF.Property case
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                if (TryBindMember(source, MemberIdentity.Create(propertyName), methodCallExpression.Type, out var result))
                {
                    return result;
                }

                throw new InvalidOperationException("EF.Property called with wrong property name.");
            }

            // EF Indexer property
            if (methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
            {
                return TryBindMember(source, MemberIdentity.Create(propertyName), methodCallExpression.Type, out var result) ? result : null;
            }

            // GroupBy Aggregate case
            if (methodCallExpression.Object == null
                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                && methodCallExpression.Arguments.Count > 0
                && methodCallExpression.Arguments[0] is InMemoryGroupByShaperExpression groupByShaperExpression)
            {
                var methodName = methodCallExpression.Method.Name;
                switch (methodName)
                {
                    case nameof(Enumerable.Average):
                    case nameof(Enumerable.Max):
                    case nameof(Enumerable.Min):
                    case nameof(Enumerable.Sum):
                    {
                        var translation = Translate(GetSelector(methodCallExpression, groupByShaperExpression));
                        if (translation == null)
                        {
                            return null;
                        }

                        var selector = Expression.Lambda(translation, groupByShaperExpression.ValueBufferParameter);
                        var method = GetMethod();
                        method = method.GetGenericArguments().Length == 2
                            ? method.MakeGenericMethod(typeof(ValueBuffer), selector.ReturnType)
                            : method.MakeGenericMethod(typeof(ValueBuffer));

                        return Expression.Call(
                            method,
                            groupByShaperExpression.GroupingParameter,
                            selector);

                        MethodInfo GetMethod()
                            => methodName switch
                            {
                                nameof(Enumerable.Average) => EnumerableMethods.GetAverageWithSelector(selector.ReturnType),
                                nameof(Enumerable.Max) => EnumerableMethods.GetMaxWithSelector(selector.ReturnType),
                                nameof(Enumerable.Min) => EnumerableMethods.GetMinWithSelector(selector.ReturnType),
                                nameof(Enumerable.Sum) => EnumerableMethods.GetSumWithSelector(selector.ReturnType),
                                _ => throw new InvalidOperationException("Invalid Aggregate Operator encountered."),
                            };
                    }

                    case nameof(Enumerable.Count):
                    case nameof(Enumerable.LongCount):
                    {
                        var countMethod = string.Equals(methodName, nameof(Enumerable.Count));
                        var predicate = GetPredicate(methodCallExpression, groupByShaperExpression);
                        if (predicate == null)
                        {
                            return Expression.Call(
                                (countMethod
                                    ? EnumerableMethods.CountWithoutPredicate
                                    : EnumerableMethods.LongCountWithoutPredicate)
                                .MakeGenericMethod(typeof(ValueBuffer)),
                                groupByShaperExpression.GroupingParameter);
                        }

                        var translation = Translate(predicate);
                        if (translation == null)
                        {
                            return null;
                        }

                        predicate = Expression.Lambda(translation, groupByShaperExpression.ValueBufferParameter);

                        return Expression.Call(
                            (countMethod
                                ? EnumerableMethods.CountWithPredicate
                                : EnumerableMethods.LongCountWithPredicate)
                            .MakeGenericMethod(typeof(ValueBuffer)),
                            groupByShaperExpression.GroupingParameter,
                            predicate);
                    }

                    default:
                        throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
                }
            }

            // Subquery case
            var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
            if (subqueryTranslation != null)
            {
                var subquery = (InMemoryQueryExpression)subqueryTranslation.QueryExpression;
                if (subqueryTranslation.ResultCardinality == ResultCardinality.Enumerable)
                {
                    return null;
                }

                subquery.ApplyProjection();
                if (subquery.Projection.Count != 1)
                {
                    return null;
                }

                // Unwrap ResultEnumerable
                var selectMethod = (MethodCallExpression)subquery.ServerQueryExpression;
                var resultEnumerable = (NewExpression)selectMethod.Arguments[0];
                var resultFunc = ((LambdaExpression)resultEnumerable.Arguments[0]).Body;
                // New ValueBuffer construct
                if (resultFunc is NewExpression newValueBufferExpression)
                {
                    Expression result;
                    var innerExpression = ((NewArrayExpression)newValueBufferExpression.Arguments[0]).Expressions[0];
                    result = innerExpression is UnaryExpression unaryExpression
                        && innerExpression.NodeType == ExpressionType.Convert
                        && innerExpression.Type == typeof(object)
                        ? unaryExpression.Operand
                        : innerExpression;

                    return result.Type == methodCallExpression.Type
                        ? result
                        : Expression.Convert(result, methodCallExpression.Type);
                }

                var selector = (LambdaExpression)selectMethod.Arguments[1];
                var readValueExpression = ((NewArrayExpression)((NewExpression)selector.Body).Arguments[0]).Expressions[0];
                if (readValueExpression is UnaryExpression unaryExpression2
                    && unaryExpression2.NodeType == ExpressionType.Convert
                    && unaryExpression2.Type == typeof(object))
                {
                    readValueExpression = unaryExpression2.Operand;
                }

                var valueBufferVariable = Expression.Variable(typeof(ValueBuffer));
                var replacedReadExpression = ReplacingExpressionVisitor.Replace(
                    selector.Parameters[0],
                    valueBufferVariable,
                    readValueExpression);

                replacedReadExpression = replacedReadExpression.Type == methodCallExpression.Type
                    ? replacedReadExpression
                    : Expression.Convert(replacedReadExpression, methodCallExpression.Type);

                return Expression.Block(
                    variables: new[] { valueBufferVariable },
                    Expression.Assign(valueBufferVariable, resultFunc),
                    Expression.Condition(
                        Expression.MakeMemberAccess(valueBufferVariable, _valueBufferIsEmpty),
                        Expression.Default(methodCallExpression.Type),
                        replacedReadExpression));
            }

            // MethodCall translators
            var @object = Visit(methodCallExpression.Object);
            if (TranslationFailed(methodCallExpression.Object, @object))
            {
                return null;
            }

            var arguments = new Expression[methodCallExpression.Arguments.Count];
            var parameterTypes = methodCallExpression.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = Visit(methodCallExpression.Arguments[i]);
                if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                {
                    return null;
                }

                // if the nullability of arguments change, we have no easy/reliable way to adjust the actual methodInfo to match the new type,
                // so we are forced to cast back to the original type
                if (IsConvertedToNullable(argument, methodCallExpression.Arguments[i])
                    && !parameterTypes[i].IsAssignableFrom(argument.Type))
                {
                    argument = ConvertToNonNullable(argument);
                }

                arguments[i] = argument;
            }

            // if object is nullable, add null safeguard before calling the function
            // we special-case Nullable<>.GetValueOrDefault, which doesn't need the safeguard
            if (methodCallExpression.Object != null
                && @object.Type.IsNullableType()
                && methodCallExpression.Method.Name != nameof(Nullable<int>.GetValueOrDefault))
            {
                var result = (Expression)methodCallExpression.Update(
                    Expression.Convert(@object, methodCallExpression.Object.Type),
                    arguments);

                result = ConvertToNullable(result);
                result = Expression.Condition(
                    Expression.Equal(@object, Expression.Constant(null, @object.Type)),
                    Expression.Constant(null, result.Type),
                    result);

                return result;
            }

            return methodCallExpression.Update(@object, arguments);
        }

        private static readonly MemberInfo _valueBufferIsEmpty = typeof(ValueBuffer).GetMember(nameof(ValueBuffer.IsEmpty))[0];

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            Check.NotNull(typeBinaryExpression, nameof(typeBinaryExpression));

            if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
                && Visit(typeBinaryExpression.Expression) is EntityProjectionExpression entityProjectionExpression)
            {
                var entityType = entityProjectionExpression.EntityType;

                if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
                {
                    return Expression.Constant(true);
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
                if (derivedType != null)
                {
                    var discriminatorProperty = entityType.GetDiscriminatorProperty();
                    var boundProperty = BindProperty(entityProjectionExpression, discriminatorProperty);

                    var equals = Expression.Equal(
                        boundProperty,
                        Expression.Constant(derivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType));

                    foreach (var derivedDerivedType in derivedType.GetDerivedTypes())
                    {
                        equals = Expression.OrElse(
                            equals,
                            Expression.Equal(
                                boundProperty,
                                Expression.Constant(derivedDerivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType)));
                    }

                    return equals;
                }
            }

            return Expression.Constant(false);
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            Check.NotNull(newExpression, nameof(newExpression));

            var newArguments = new List<Expression>();
            foreach (var argument in newExpression.Arguments)
            {
                var newArgument = Visit(argument);
                if (newArgument == null)
                {
                    return null;
                }

                if (IsConvertedToNullable(newArgument, argument))
                {
                    newArgument = ConvertToNonNullable(newArgument);
                }

                newArguments.Add(newArgument);
            }

            return newExpression.Update(newArguments);
        }

        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
        {
            Check.NotNull(newArrayExpression, nameof(newArrayExpression));

            var newExpressions = new List<Expression>();
            foreach (var expression in newArrayExpression.Expressions)
            {
                var newExpression = Visit(expression);
                if (newExpression == null)
                {
                    return null;
                }

                if (IsConvertedToNullable(newExpression, expression))
                {
                    newExpression = ConvertToNonNullable(newExpression);
                }

                newExpressions.Add(newExpression);
            }

            return newArrayExpression.Update(newExpressions);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
        {
            var expression = Visit(memberAssignment.Expression);
            if (expression == null)
            {
                return null;
            }

            if (IsConvertedToNullable(expression, memberAssignment.Expression))
            {
                expression = ConvertToNonNullable(expression);
            }

            return memberAssignment.Update(expression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return Visit(entityShaperExpression.ValueBufferExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    return projectionBindingExpression.ProjectionMember != null
                        ? ((InMemoryQueryExpression)projectionBindingExpression.QueryExpression)
                        .GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : null;

                default:
                    return null;
            }
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            if (parameterExpression.Name.StartsWith(CompiledQueryParameterPrefix, StringComparison.Ordinal))
            {
                return Expression.Call(
                    _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                    QueryCompilationContext.QueryContextParameter,
                    Expression.Constant(parameterExpression.Name));
            }

            throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));
        }

        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(InMemoryExpressionTranslatingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
            => (T)queryContext.ParameterValues[parameterName];

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            var newOperand = Visit(unaryExpression.Operand);
            if (newOperand == null)
            {
                return null;
            }

            if (unaryExpression.NodeType == ExpressionType.Convert
                && newOperand.Type == unaryExpression.Type)
            {
                return newOperand;
            }

            if (unaryExpression.NodeType == ExpressionType.Convert
                && IsConvertedToNullable(newOperand, unaryExpression))
            {
                return newOperand;
            }

            var result = (Expression)Expression.MakeUnary(unaryExpression.NodeType, newOperand, unaryExpression.Type);
            if (result is UnaryExpression outerUnary
                && outerUnary.NodeType == ExpressionType.Convert
                && outerUnary.Operand is UnaryExpression innerUnary
                && innerUnary.NodeType == ExpressionType.Convert)
            {
                var innerMostType = innerUnary.Operand.Type;
                var intermediateType = innerUnary.Type;
                var outerMostType = outerUnary.Type;

                if (outerMostType == innerMostType
                    && intermediateType == innerMostType.UnwrapNullableType())
                {
                    result = innerUnary.Operand;
                }
                else if (outerMostType == typeof(object)
                    && intermediateType == innerMostType.UnwrapNullableType())
                {
                    result = Expression.Convert(innerUnary.Operand, typeof(object));
                }
            }

            return result;
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation)
            => original != null && (translation == null || translation is EntityProjectionExpression);
    }
}
