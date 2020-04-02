// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
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
    public class InMemoryExpressionTranslatingExpressionVisitor : ExpressionVisitor
    {
        private const string _runtimeParameterPrefix = QueryCompilationContext.QueryParameterPrefix + "entity_equality_";

        private static readonly MemberInfo _valueBufferIsEmpty = typeof(ValueBuffer).GetMember(nameof(ValueBuffer.IsEmpty))[0];

        private static readonly MethodInfo _parameterValueExtractor =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor));
        private static readonly MethodInfo _parameterListValueExtractor =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor));
        private static readonly MethodInfo _getParameterValueMethodInfo =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));
        private static readonly MethodInfo _likeMethodInfo = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });
        private static readonly MethodInfo _likeMethodInfoWithEscape = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });
        private static readonly MethodInfo _inMemoryLikeMethodInfo =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InMemoryLike));

        // Regex special chars defined here:
        // https://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx
        private static readonly char[] _regexSpecialChars
            = { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\' };

        private static readonly string _defaultEscapeRegexCharsPattern = BuildEscapeRegexCharsPattern(_regexSpecialChars);

        private static readonly TimeSpan _regexTimeout = TimeSpan.FromMilliseconds(value: 1000.0);
        private static string BuildEscapeRegexCharsPattern(IEnumerable<char> regexSpecialChars)
            => string.Join("|", regexSpecialChars.Select(c => @"\" + c));

        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly EntityReferenceFindingExpressionVisitor _entityReferenceFindingExpressionVisitor;
        private readonly IModel _model;

        public InMemoryExpressionTranslatingExpressionVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        {
            _queryCompilationContext = queryCompilationContext;
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _entityReferenceFindingExpressionVisitor = new EntityReferenceFindingExpressionVisitor();
            _model = queryCompilationContext.Model;
        }

        public virtual Expression Translate([NotNull] Expression expression)
        {
            var result = Visit(expression);

            return _entityReferenceFindingExpressionVisitor.Find(result)
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

            if ((binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
                // Visited expression could be null, We need to pass MemberInitExpression
                && TryRewriteEntityEquality(
                    binaryExpression.NodeType, newLeft ?? binaryExpression.Left, newRight ?? binaryExpression.Right, out var result))
            {
                return result;
            }

            if (IsConvertedToNullable(newLeft, binaryExpression.Left)
                || IsConvertedToNullable(newRight, binaryExpression.Right))
            {
                newLeft = ConvertToNullable(newLeft);
                newRight = ConvertToNullable(newRight);
            }

            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var property = FindProperty(newLeft) ?? FindProperty(newRight);
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

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                case EntityReferenceExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return new EntityReferenceExpression(entityShaperExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    return projectionBindingExpression.ProjectionMember != null
                        ? ((InMemoryQueryExpression)projectionBindingExpression.QueryExpression)
                            .GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : null;

                default:
                    return null;
            }
        }

        protected override Expression VisitInvocation(InvocationExpression invocationExpression) => null;
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression) => null;
        protected override Expression VisitListInit(ListInitExpression listInitExpression) => null;

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var innerExpression = Visit(memberExpression.Expression);
            if (memberExpression.Expression != null
                && innerExpression == null)
            {
                return null;
            }

            if (TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member), memberExpression.Type) is Expression result)
            {
                return result;
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

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                return methodCallExpression;
            }

            // EF.Property case
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                return TryBindMember(Visit(source), MemberIdentity.Create(propertyName), methodCallExpression.Type)
                    ?? throw new InvalidOperationException(CoreStrings.QueryUnableToTranslateEFProperty(methodCallExpression.Print()));
            }

            // EF Indexer property
            if (methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
            {
                return TryBindMember(Visit(source), MemberIdentity.Create(propertyName), methodCallExpression.Type);
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
                        var translation = Translate(GetSelectorOnGrouping(methodCallExpression, groupByShaperExpression));
                        if (translation == null)
                        {
                            return null;
                        }

                        var selector = Expression.Lambda(translation, groupByShaperExpression.ValueBufferParameter);
                        var method2 = GetMethod();
                        method2 = method2.GetGenericArguments().Length == 2
                            ? method2.MakeGenericMethod(typeof(ValueBuffer), selector.ReturnType)
                            : method2.MakeGenericMethod(typeof(ValueBuffer));

                        return Expression.Call(
                            method2,
                            groupByShaperExpression.GroupingParameter,
                            selector);

                        MethodInfo GetMethod()
                            => methodName switch
                            {
                                nameof(Enumerable.Average) => EnumerableMethods.GetAverageWithSelector(selector.ReturnType),
                                nameof(Enumerable.Max) => EnumerableMethods.GetMaxWithSelector(selector.ReturnType),
                                nameof(Enumerable.Min) => EnumerableMethods.GetMinWithSelector(selector.ReturnType),
                                nameof(Enumerable.Sum) => EnumerableMethods.GetSumWithSelector(selector.ReturnType),
                                _ => throw new InvalidOperationException(InMemoryStrings.InvalidStateEncountered("Aggregate Operator")),
                            };
                    }

                    case nameof(Enumerable.Count):
                    case nameof(Enumerable.LongCount):
                    {
                        var countMethod = string.Equals(methodName, nameof(Enumerable.Count));
                        var predicate = GetPredicateOnGrouping(methodCallExpression, groupByShaperExpression);
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

                if (subqueryTranslation.ShaperExpression is EntityShaperExpression entityShaperExpression)
                {
                    return new EntityReferenceExpression(subqueryTranslation);
                }

#pragma warning disable IDE0046 // Convert to conditional expression
                if (!(subqueryTranslation.ShaperExpression is ProjectionBindingExpression projectionBindingExpression))
#pragma warning restore IDE0046 // Convert to conditional expression
                {
                    return null;
                }

                return ProcessSingleResultScalar(subquery.ServerQueryExpression,
                    subquery.GetMappedProjection(projectionBindingExpression.ProjectionMember),
                    subquery.CurrentParameter,
                    methodCallExpression.Type);
            }

            if (methodCallExpression.Method == _likeMethodInfo
                || methodCallExpression.Method == _likeMethodInfoWithEscape)
            {
                // EF.Functions.Like
                var visitedArguments = new Expression[3];
                visitedArguments[2] = Expression.Constant(null, typeof(string));
                // Skip first DbFunctions argument
                for (var i = 1; i < methodCallExpression.Arguments.Count; i++)
                {
                    var argument = Visit(methodCallExpression.Arguments[i]);
                    if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                    {
                        return null;
                    }

                    visitedArguments[i - 1] = argument;
                }

                return Expression.Call(_inMemoryLikeMethodInfo, visitedArguments);
            }

            Expression @object = null;
            Expression[] arguments;
            var method = methodCallExpression.Method;

            if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object != null
                && methodCallExpression.Arguments.Count == 1)
            {
                var left = Visit(methodCallExpression.Object);
                var right = Visit(methodCallExpression.Arguments[0]);

                if (TryRewriteEntityEquality(ExpressionType.Equal,
                        left ?? methodCallExpression.Object,
                        right ?? methodCallExpression.Arguments[0],
                        out var result))
                {
                    return result;
                }

                if (TranslationFailed(left)
                    || TranslationFailed(right))
                {
                    return null;
                }

                @object = left;
                arguments = new Expression[1] { right };
            }
            else if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object == null
                && methodCallExpression.Arguments.Count == 2)
            {
                var left = Visit(methodCallExpression.Arguments[0]);
                var right = Visit(methodCallExpression.Arguments[1]);

                if (TryRewriteEntityEquality(ExpressionType.Equal,
                    left ?? methodCallExpression.Arguments[0],
                    right ?? methodCallExpression.Arguments[1],
                    out var result))
                {
                    return result;
                }

                if (TranslationFailed(left)
                    || TranslationFailed(right))
                {
                    return null;
                }

                arguments = new Expression[2] { left, right };
            }
            else if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
            {
                var enumerable = Visit(methodCallExpression.Arguments[0]);
                var item = Visit(methodCallExpression.Arguments[1]);

                if (TryRewriteContainsEntity(enumerable, item ?? methodCallExpression.Arguments[1], out var result))
                {
                    return result;
                }

                if (TranslationFailed(enumerable)
                    || TranslationFailed(item))
                {
                    return null;
                }

                arguments = new Expression[2] { enumerable, item };
            }
            else if (methodCallExpression.Arguments.Count == 1
                && method.IsContainsMethod())
            {
                var enumerable = Visit(methodCallExpression.Object);
                var item = Visit(methodCallExpression.Arguments[0]);

                if (TryRewriteContainsEntity(enumerable, item ?? methodCallExpression.Arguments[0], out var result))
                {
                    return result;
                }

                if (TranslationFailed(enumerable)
                    || TranslationFailed(item))
                {
                    return null;
                }

                @object = enumerable;
                arguments = new Expression[1] { item };
            }
            else
            {
                @object = Visit(methodCallExpression.Object);
                if (TranslationFailed(methodCallExpression.Object, @object))
                {
                    return null;
                }

                arguments = new Expression[methodCallExpression.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = Visit(methodCallExpression.Arguments[i]);
                    if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                    {
                        return null;
                    }

                    arguments[i] = argument;
                }
            }

            // if the nullability of arguments change, we have no easy/reliable way to adjust the actual methodInfo to match the new type,
            // so we are forced to cast back to the original type
            var parameterTypes = methodCallExpression.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
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

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            if (parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true)
            {
                return Expression.Call(
                    _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                    QueryCompilationContext.QueryContextParameter,
                    Expression.Constant(parameterExpression.Name));
            }

            throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            Check.NotNull(typeBinaryExpression, nameof(typeBinaryExpression));

            if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
                && Visit(typeBinaryExpression.Expression) is EntityReferenceExpression entityReferenceExpression)
            {
                var entityType = entityReferenceExpression.EntityType;

                if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
                {
                    return Expression.Constant(true);
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
                if (derivedType != null)
                {
                    var discriminatorProperty = entityType.GetDiscriminatorProperty();
                    var boundProperty = BindProperty(entityReferenceExpression, discriminatorProperty, discriminatorProperty.ClrType);

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

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            var newOperand = Visit(unaryExpression.Operand);
            if (newOperand == null)
            {
                return null;
            }

            if (newOperand is EntityReferenceExpression entityReferenceExpression
                && (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked
                    || unaryExpression.NodeType == ExpressionType.TypeAs))
            {
                return entityReferenceExpression.Convert(unaryExpression.Type);
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

        private Expression TryBindMember(Expression source, MemberIdentity member, Type type)
        {
            if (!(source is EntityReferenceExpression entityReferenceExpression))
            {
                return null;
            }

            var entityType = entityReferenceExpression.EntityType;

            var property = member.MemberInfo != null
                ? entityType.FindProperty(member.MemberInfo)
                : entityType.FindProperty(member.Name);

            return property != null ? BindProperty(entityReferenceExpression, property, type) : null;
        }

        private Expression BindProperty(EntityReferenceExpression entityReferenceExpression, IProperty property, Type type)
        {
            if (entityReferenceExpression.ParameterEntity != null)
            {
                var result = ((EntityProjectionExpression)Visit(entityReferenceExpression.ParameterEntity.ValueBufferExpression))
                    .BindProperty(property);

                // if the result type change was just nullability change e.g from int to int?
                // we want to preserve the new type for null propagation
                if (result.Type != type
                    && !(result.Type.IsNullableType()
                        && !type.IsNullableType()
                        && result.Type.UnwrapNullableType() == type))
                {
                    result = Expression.Convert(result, type);
                }

                return result;
            }

            if (entityReferenceExpression.SubqueryEntity != null)
            {
                var entityShaper = (EntityShaperExpression)entityReferenceExpression.SubqueryEntity.ShaperExpression;
                var readValueExpression = ((EntityProjectionExpression)Visit(entityShaper.ValueBufferExpression)).BindProperty(property);
                var inMemoryQueryExpression = (InMemoryQueryExpression)entityReferenceExpression.SubqueryEntity.QueryExpression;

                return ProcessSingleResultScalar(
                    inMemoryQueryExpression.ServerQueryExpression,
                    readValueExpression,
                    inMemoryQueryExpression.CurrentParameter,
                    type);
            }

            return null;
        }

        private static Expression ProcessSingleResultScalar(
            Expression serverQuery, Expression readValueExpression, Expression valueBufferParameter, Type type)
        {
            var singleResult = ((LambdaExpression)((NewExpression)serverQuery).Arguments[0]).Body;
            if (readValueExpression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Type == typeof(object))
            {
                readValueExpression = unaryExpression.Operand;
            }

            var valueBufferVariable = Expression.Variable(typeof(ValueBuffer));
            var replacedReadExpression = ReplacingExpressionVisitor.Replace(
                valueBufferParameter,
                valueBufferVariable,
                readValueExpression);

            replacedReadExpression = replacedReadExpression.Type == type
                ? replacedReadExpression
                : Expression.Convert(replacedReadExpression, type);

            return Expression.Block(
                variables: new[] { valueBufferVariable },
                Expression.Assign(valueBufferVariable, singleResult),
                Expression.Condition(
                    Expression.MakeMemberAccess(valueBufferVariable, _valueBufferIsEmpty),
                    Expression.Default(type),
                    replacedReadExpression));
        }

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
            => (T)queryContext.ParameterValues[parameterName];

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

        private static Expression GetSelectorOnGrouping(
            MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
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

        private Expression GetPredicateOnGrouping(
            MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
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

        private IProperty FindProperty(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert
                && expression.Type.IsNullableType()
                && expression is UnaryExpression unaryExpression
                && expression.Type.UnwrapNullableType() == unaryExpression.Type)
            {
                expression = unaryExpression.Operand;
            }

            if (expression is MethodCallExpression readValueMethodCall
                && readValueMethodCall.Method.IsGenericMethod
                && readValueMethodCall.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                return (IProperty)((ConstantExpression)readValueMethodCall.Arguments[2]).Value;
            }

            return null;
        }

        private bool TryRewriteContainsEntity(Expression source, Expression item, out Expression result)
        {
            result = null;

            if (!(item is EntityReferenceExpression itemEntityReference))
            {
                return false;
            }

            var entityType = itemEntityReference.EntityType;
            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(entityType.DisplayName()));
            }

            if (primaryKeyProperties.Count > 1)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityContainsWithCompositeKeyNotSupported(entityType.DisplayName()));
            }

            var property = primaryKeyProperties[0];
            Expression rewrittenSource;
            switch (source)
            {
                case ConstantExpression constantExpression:
                    var values = (IEnumerable)constantExpression.Value;
                    var propertyValueList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.ClrType.MakeNullable()));
                    var propertyGetter = property.GetGetter();
                    foreach (var value in values)
                    {
                        propertyValueList.Add(propertyGetter.GetClrValue(value));
                    }

                    rewrittenSource = Expression.Constant(propertyValueList);
                    break;

                case MethodCallExpression methodCallExpression
                when methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == _getParameterValueMethodInfo:
                    var parameterName = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterListValueExtractor.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(parameterName, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter
                    );

                    var newParameterName =
                        $"{_runtimeParameterPrefix}" +
                        $"{parameterName.Substring(QueryCompilationContext.QueryParameterPrefix.Length)}_{property.Name}";

                    rewrittenSource = _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                    break;

                default:
                    return false;
            }

            result = Visit(Expression.Call(
                EnumerableMethods.Contains.MakeGenericMethod(property.ClrType.MakeNullable()),
                rewrittenSource,
                CreatePropertyAccessExpression(item, property)));

            return true;
        }

        private bool TryRewriteEntityEquality(ExpressionType nodeType, Expression left, Expression right, out Expression result)
        {
            var leftEntityReference = left as EntityReferenceExpression;
            var rightEntityReference = right as EntityReferenceExpression;

            if (leftEntityReference == null
                && rightEntityReference == null)
            {
                result = null;
                return false;
            }

            if (IsNullConstantExpression(left)
                || IsNullConstantExpression(right))
            {
                var nonNullEntityReference = IsNullConstantExpression(left) ? rightEntityReference : leftEntityReference;
                var entityType1 = nonNullEntityReference.EntityType;
                var primaryKeyProperties1 = entityType1.FindPrimaryKey()?.Properties;
                if (primaryKeyProperties1 == null)
                {
                    throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(entityType1.DisplayName()));
                }

                result = Visit(primaryKeyProperties1.Select(p =>
                    Expression.MakeBinary(
                        nodeType, CreatePropertyAccessExpression(nonNullEntityReference, p), Expression.Constant(null, p.ClrType.MakeNullable())))
                    .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

                return true;
            }

            var leftEntityType = leftEntityReference?.EntityType;
            var rightEntityType = rightEntityReference?.EntityType;
            var entityType = leftEntityType ?? rightEntityType;

            Debug.Assert(entityType != null, "At least either side should be entityReference so entityType should be non-null.");

            if (leftEntityType != null
                && rightEntityType != null
                && leftEntityType.GetRootType() != rightEntityType.GetRootType())
            {
                result = Expression.Constant(false);
                return true;
            }

            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(entityType.DisplayName()));
            }

            if (primaryKeyProperties.Count > 1
                && (leftEntityReference?.SubqueryEntity != null
                    || rightEntityReference?.SubqueryEntity != null))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualitySubqueryWithCompositeKeyNotSupported(entityType.DisplayName()));
            }

            result = Visit(primaryKeyProperties.Select(p =>
                    Expression.MakeBinary(
                        nodeType,
                        CreatePropertyAccessExpression(left, p),
                        CreatePropertyAccessExpression(right, p)))
                    .Aggregate((l, r) => Expression.AndAlso(l, r)));

            return true;
        }

        private Expression CreatePropertyAccessExpression(Expression target, IProperty property)
        {
            switch (target)
            {
                case ConstantExpression constantExpression:
                    return Expression.Constant(
                        property.GetGetter().GetClrValue(constantExpression.Value), property.ClrType.MakeNullable());

                case MethodCallExpression methodCallExpression
                when methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == _getParameterValueMethodInfo:
                    var parameterName = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterValueExtractor.MakeGenericMethod(property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(parameterName, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter);

                    var newParameterName =
                        $"{_runtimeParameterPrefix}" +
                        $"{parameterName.Substring(QueryCompilationContext.QueryParameterPrefix.Length)}_{property.Name}";

                    return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);

                case MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.SingleOrDefault(
                    mb => mb.Member.Name == property.Name) is MemberAssignment memberAssignment:
                    return memberAssignment.Expression.Type.IsNullableType()
                        ? memberAssignment.Expression
                        : Expression.Convert(memberAssignment.Expression, property.ClrType.MakeNullable());

                case NewExpression newExpression
                when CanEvaluate(newExpression):
                    return CreatePropertyAccessExpression(GetValue(newExpression), property);

                case MemberInitExpression memberInitExpression
                when CanEvaluate(memberInitExpression):
                    return CreatePropertyAccessExpression(GetValue(memberInitExpression), property);

                default:
                    return target.CreateEFPropertyExpression(property);
            }
        }

        private static T ParameterValueExtractor<T>(QueryContext context, string baseParameterName, IProperty property)
        {
            var baseParameter = context.ParameterValues[baseParameterName];
            return baseParameter == null ? (T)(object)null : (T)property.GetGetter().GetClrValue(baseParameter);
        }

        private static List<TProperty> ParameterListValueExtractor<TEntity, TProperty>(
            QueryContext context, string baseParameterName, IProperty property)
        {
            if (!(context.ParameterValues[baseParameterName] is IEnumerable<TEntity> baseListParameter))
            {
                return null;
            }

            var getter = property.GetGetter();
            return baseListParameter.Select(e => e != null ? (TProperty)getter.GetClrValue(e) : (TProperty)(object)null).ToList();
        }

        private static ConstantExpression GetValue(Expression expression)
            => Expression.Constant(
                Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke(),
                expression.Type);

        private static bool CanEvaluate(Expression expression)
        {
#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (expression)
#pragma warning restore IDE0066 // Convert switch statement to expression
            {
                case ConstantExpression constantExpression:
                    return true;

                case NewExpression newExpression:
                    return newExpression.Arguments.All(e => CanEvaluate(e));

                case MemberInitExpression memberInitExpression:
                    return CanEvaluate(memberInitExpression.NewExpression)
                        && memberInitExpression.Bindings.All(
                            mb => mb is MemberAssignment memberAssignment && CanEvaluate(memberAssignment.Expression));

                default:
                    return false;
            }
        }

        private static bool IsNullConstantExpression(Expression expression)
            => expression is ConstantExpression constantExpression && constantExpression.Value == null;

        [DebuggerStepThrough]
        private static bool TranslationFailed(Expression original, Expression translation)
            => original != null && (translation == null || translation is EntityReferenceExpression);

        private static bool TranslationFailed(Expression translation)
            => translation == null || translation is EntityReferenceExpression;

        private static bool InMemoryLike(string matchExpression, string pattern, string escapeCharacter)
        {
            //TODO: this fixes https://github.com/aspnet/EntityFramework/issues/8656 by insisting that
            // the "escape character" is a string but just using the first character of that string,
            // but we may later want to allow the complete string as the "escape character"
            // in which case we need to change the way we construct the regex below.
            var singleEscapeCharacter =
                (escapeCharacter == null || escapeCharacter.Length == 0)
                    ? (char?)null
                    : escapeCharacter.First();

            if (matchExpression == null
                || pattern == null)
            {
                return false;
            }

            if (matchExpression.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (matchExpression.Length == 0
                || pattern.Length == 0)
            {
                return false;
            }

            var escapeRegexCharsPattern
                = singleEscapeCharacter == null
                    ? _defaultEscapeRegexCharsPattern
                    : BuildEscapeRegexCharsPattern(_regexSpecialChars.Where(c => c != singleEscapeCharacter));

            var regexPattern
                = Regex.Replace(
                    pattern,
                    escapeRegexCharsPattern,
                    c => @"\" + c,
                    default,
                    _regexTimeout);

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < regexPattern.Length; i++)
            {
                var c = regexPattern[i];
                var escaped = i > 0 && regexPattern[i - 1] == singleEscapeCharacter;

                switch (c)
                {
                    case '_':
                    {
                        stringBuilder.Append(escaped ? '_' : '.');
                        break;
                    }
                    case '%':
                    {
                        stringBuilder.Append(escaped ? "%" : ".*");
                        break;
                    }
                    default:
                    {
                        if (c != singleEscapeCharacter)
                        {
                            stringBuilder.Append(c);
                        }

                        break;
                    }
                }
            }

            regexPattern = stringBuilder.ToString();

            return Regex.IsMatch(
                matchExpression,
                @"\A" + regexPattern + @"\s*\z",
                RegexOptions.IgnoreCase | RegexOptions.Singleline,
                _regexTimeout);
        }

        private sealed class EntityReferenceFindingExpressionVisitor : ExpressionVisitor
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

                if (expression is EntityReferenceExpression)
                {
                    _found = true;
                    return expression;
                }

                return base.Visit(expression);
            }
        }

        private sealed class EntityReferenceExpression : Expression
        {
            public EntityReferenceExpression(EntityShaperExpression parameter)
            {
                ParameterEntity = parameter;
                EntityType = parameter.EntityType;
            }

            public EntityReferenceExpression(ShapedQueryExpression subquery)
            {
                SubqueryEntity = subquery;
                EntityType = ((EntityShaperExpression)subquery.ShaperExpression).EntityType;
            }

            private EntityReferenceExpression(EntityReferenceExpression entityReferenceExpression, IEntityType entityType)
            {
                ParameterEntity = entityReferenceExpression.ParameterEntity;
                SubqueryEntity = entityReferenceExpression.SubqueryEntity;
                EntityType = entityType;
            }

            public EntityShaperExpression ParameterEntity { get; }
            public ShapedQueryExpression SubqueryEntity { get; }
            public IEntityType EntityType { get; }

            public override Type Type => EntityType.ClrType;
            public override ExpressionType NodeType => ExpressionType.Extension;

            public Expression Convert(Type type)
            {
                if (type == typeof(object) // Ignore object conversion
                    || type.IsAssignableFrom(Type)) // Ignore casting to base type/interface
                {
                    return this;
                }

                var derivedEntityType = EntityType.GetDerivedTypes().FirstOrDefault(et => et.ClrType == type);

                return derivedEntityType == null ? null : new EntityReferenceExpression(this, derivedEntityType);
            }
        }
    }
}
