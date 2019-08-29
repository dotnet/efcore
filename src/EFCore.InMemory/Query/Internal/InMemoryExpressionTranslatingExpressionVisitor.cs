// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryExpressionTranslatingExpressionVisitor : ExpressionVisitor
    {
        private const string CompiledQueryParameterPrefix = "__";

        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly EntityProjectionFindingExpressionVisitor _entityProjectionFindingExpressionVisitor;

        public InMemoryExpressionTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        {
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _entityProjectionFindingExpressionVisitor = new EntityProjectionFindingExpressionVisitor();
        }

        private class EntityProjectionFindingExpressionVisitor : ExpressionVisitor
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

        public virtual Expression Translate(Expression expression)
        {
            var result = Visit(expression);

            return _entityProjectionFindingExpressionVisitor.Find(result)
                ? null
                : result;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

            if (newLeft == null || newRight == null)
            {
                return null;
            }

            if (IsConvertedToNullable(newLeft, binaryExpression.Left)
                || IsConvertedToNullable(newRight, binaryExpression.Right))
            {
                newLeft = ConvertToNullable(newLeft);
                newRight = ConvertToNullable(newRight);
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
            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            if (test == null || ifTrue == null || ifFalse == null)
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
            var innerExpression = Visit(memberExpression.Expression);
            if (memberExpression.Expression != null && innerExpression == null)
            {
                return null;
            }

            if ((innerExpression is EntityProjectionExpression
                || (innerExpression is UnaryExpression innerUnaryExpression
                    && innerUnaryExpression.NodeType == ExpressionType.Convert
                    && innerUnaryExpression.Operand is EntityProjectionExpression))
                && TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member), memberExpression.Type, out var result))
            {
                return result;
            }

            static bool shouldApplyNullProtectionForMemberAccess(Type callerType, string memberName)
                => !(callerType.IsGenericType
                    && callerType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && (memberName == nameof(Nullable<int>.Value) || memberName == nameof(Nullable<int>.HasValue)));

            var updatedMemberExpression = (Expression)memberExpression.Update(innerExpression);
            if (innerExpression != null
                && innerExpression.Type.IsNullableType()
                && shouldApplyNullProtectionForMemberAccess(innerExpression.Type, memberExpression.Member.Name))
            {
                updatedMemberExpression = ConvertToNullable(updatedMemberExpression);

                return Expression.Condition(
                    Expression.Equal(innerExpression, Expression.Default(innerExpression.Type)),
                    Expression.Default(updatedMemberExpression.Type),
                    updatedMemberExpression);
            }

            return updatedMemberExpression;
        }

        private bool TryBindMember(Expression source, MemberIdentity memberIdentity, Type type, out Expression result)
        {
            result = null;
            Type convertedType = null;
            if (source is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                source = unaryExpression.Operand;
                if (unaryExpression.Type != typeof(object))
                {
                    convertedType = unaryExpression.Type;
                }
            }

            if (source is EntityProjectionExpression entityProjection)
            {
                var entityType = entityProjection.EntityType;
                if (convertedType != null
                    && !(convertedType.IsInterface
                         && convertedType.IsAssignableFrom(entityType.ClrType)))
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
                // If unmapped property return null
                if (property == null)
                {
                    return false;
                }

                result = BindProperty(entityProjection, property);

                // if the result type change was just nullability change e.g from int to int? we want to preserve the new type for null propagation
                if (result.Type != type
                    && !(result.Type.IsNullableType()
                        && !type.IsNullableType()
                        && result.Type.UnwrapNullableType() == type))
                {
                    result = Expression.Convert(result, type);
                }

                return true;
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

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
            {
                return methodCallExpression;
            }

            // EF.Property case
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                if (TryBindMember(Visit(source), MemberIdentity.Create(propertyName), methodCallExpression.Type, out var result))
                {
                    return result;
                }

                throw new InvalidOperationException("EF.Property called with wrong property name.");
            }

            // GroupBy Aggregate case
            if (methodCallExpression.Object == null
                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                && methodCallExpression.Arguments.Count > 0
                && methodCallExpression.Arguments[0] is InMemoryGroupByShaperExpression groupByShaperExpression)
            {
                switch (methodCallExpression.Method.Name)
                {
                    case nameof(Enumerable.Average):
                    case nameof(Enumerable.Max):
                    case nameof(Enumerable.Min):
                    case nameof(Enumerable.Sum):
                        var translation = Translate(GetSelector(methodCallExpression, groupByShaperExpression));
                        var selector = Expression.Lambda(translation, groupByShaperExpression.ValueBufferParameter);
                        MethodInfo getMethod()
                            => methodCallExpression.Method.Name switch
                            {
                                nameof(Enumerable.Average) => InMemoryLinqOperatorProvider.GetAverageWithSelector(selector.ReturnType),
                                nameof(Enumerable.Max) => InMemoryLinqOperatorProvider.GetMaxWithSelector(selector.ReturnType),
                                nameof(Enumerable.Min) => InMemoryLinqOperatorProvider.GetMinWithSelector(selector.ReturnType),
                                nameof(Enumerable.Sum) => InMemoryLinqOperatorProvider.GetSumWithSelector(selector.ReturnType),
                                _ => throw new InvalidOperationException("Invalid Aggregate Operator encountered."),
                            };
                        var method = getMethod();
                        method = method.GetGenericArguments().Length == 2
                            ? method.MakeGenericMethod(typeof(ValueBuffer), selector.ReturnType)
                            : method.MakeGenericMethod(typeof(ValueBuffer));

                        return Expression.Call(method,
                            groupByShaperExpression.GroupingParameter,
                            selector);

                    case nameof(Enumerable.Count):
                        return Expression.Call(
                            InMemoryLinqOperatorProvider.CountWithoutPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                            groupByShaperExpression.GroupingParameter);
                    case nameof(Enumerable.LongCount):
                        return Expression.Call(
                            InMemoryLinqOperatorProvider.LongCountWithoutPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                            groupByShaperExpression.GroupingParameter);

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

                Expression result;

                // Unwrap ResultEnumerable
                var selectMethod = (MethodCallExpression)subquery.ServerQueryExpression;
                var resultEnumerable = (NewExpression)selectMethod.Arguments[0];
                var resultFunc = ((LambdaExpression)resultEnumerable.Arguments[0]).Body;
                // New ValueBuffer construct
                if (resultFunc is NewExpression newValueBufferExpression)
                {
                    var innerExpression = ((NewArrayExpression)newValueBufferExpression.Arguments[0]).Expressions[0];
                    if (innerExpression is UnaryExpression unaryExpression
                        && innerExpression.NodeType == ExpressionType.Convert
                        && innerExpression.Type == typeof(object))
                    {
                        result = unaryExpression.Operand;
                    }
                    else
                    {
                        result = innerExpression;
                    }

                    return result.Type == methodCallExpression.Type
                        ? result
                        : Expression.Convert(result, methodCallExpression.Type);
                }
                else
                {
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
                && !(methodCallExpression.Method.Name == nameof(Nullable<int>.GetValueOrDefault)))
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
            var newArguments = new List<Expression>();
            foreach (var argument in newExpression.Arguments)
            {
                var newArgument = Visit(argument);
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
            var newExpressions = new List<Expression>();
            foreach (var expression in newArrayExpression.Expressions)
            {
                var newExpression = Visit(expression);
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
            if (IsConvertedToNullable(expression, memberAssignment.Expression))
            {
                expression = ConvertToNonNullable(expression);
            }

            return memberAssignment.Update(expression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return Visit(entityShaperExpression.ValueBufferExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    return ((InMemoryQueryExpression)projectionBindingExpression.QueryExpression)
                        .GetMappedProjection(projectionBindingExpression.ProjectionMember);

                case NullConditionalExpression nullConditionalExpression:
                {
                    var translation = Visit(nullConditionalExpression.AccessOperation);

                    return translation.Type == nullConditionalExpression.Type
                        ? translation
                        : Expression.Convert(translation, nullConditionalExpression.Type);
                }

                default:
                    return null;
            }
        }

        protected override Expression VisitListInit(ListInitExpression node) => null;

        protected override Expression VisitInvocation(InvocationExpression node) => null;

        protected override Expression VisitLambda<T>(Expression<T> node) => null;

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
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

#pragma warning disable IDE0052 // Remove unread private members
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
#pragma warning restore IDE0052 // Remove unread private members
            => (T)queryContext.ParameterValues[parameterName];

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var newOperand = Visit(unaryExpression.Operand);

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
