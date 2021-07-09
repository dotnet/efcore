// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public class InMemoryExpressionTranslatingExpressionVisitor : ExpressionVisitor
    {
        private const string _runtimeParameterPrefix = QueryCompilationContext.QueryParameterPrefix + "entity_equality_";

        private static readonly MemberInfo _valueBufferIsEmpty = typeof(ValueBuffer).GetMember(nameof(ValueBuffer.IsEmpty))[0];

        private static readonly MethodInfo _parameterValueExtractor =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetRequiredDeclaredMethod(nameof(ParameterValueExtractor));

        private static readonly MethodInfo _parameterListValueExtractor =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetRequiredDeclaredMethod(nameof(ParameterListValueExtractor));

        private static readonly MethodInfo _getParameterValueMethodInfo =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetRequiredDeclaredMethod(nameof(GetParameterValue));

        private static readonly MethodInfo _likeMethodInfo = typeof(DbFunctionsExtensions).GetRequiredRuntimeMethod(
            nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _likeMethodInfoWithEscape = typeof(DbFunctionsExtensions).GetRequiredRuntimeMethod(
            nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        private static readonly MethodInfo _randomMethodInfo = typeof(DbFunctionsExtensions).GetRequiredRuntimeMethod(
            nameof(DbFunctionsExtensions.Random), new[] { typeof(DbFunctions) });

        private static readonly MethodInfo _randomNextDoubleMethodInfo = typeof(Random).GetRequiredRuntimeMethod(
            nameof(Random.NextDouble), Array.Empty<Type>());

        private static readonly MethodInfo _inMemoryLikeMethodInfo =
            typeof(InMemoryExpressionTranslatingExpressionVisitor).GetRequiredDeclaredMethod(nameof(InMemoryLike));

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryExpressionTranslatingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        {
            _queryCompilationContext = queryCompilationContext;
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _entityReferenceFindingExpressionVisitor = new EntityReferenceFindingExpressionVisitor();
            _model = queryCompilationContext.Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? TranslationErrorDetails { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void AddTranslationErrorDetails(string details)
        {
            Check.NotNull(details, nameof(details));

            if (TranslationErrorDetails == null)
            {
                TranslationErrorDetails = details;
            }
            else
            {
                TranslationErrorDetails += Environment.NewLine + details;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression? Translate(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            TranslationErrorDetails = null;

            return TranslateInternal(expression);
        }

        private Expression? TranslateInternal(Expression expression)
        {
            var result = Visit(expression);

            return result == QueryCompilationContext.NotTranslatedExpression
                || _entityReferenceFindingExpressionVisitor.Find(result)
                ? null
                : result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.Left.Type == typeof(object[])
                && binaryExpression.Left is NewArrayExpression
                && binaryExpression.NodeType == ExpressionType.Equal)
            {
                return Visit(ConvertObjectArrayEqualityComparison(binaryExpression.Left, binaryExpression.Right));
            }

            var newLeft = Visit(binaryExpression.Left);
            var newRight = Visit(binaryExpression.Right);

            if (newLeft == QueryCompilationContext.NotTranslatedExpression
                || newRight == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            if ((binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
                // Visited expression could be null, We need to pass MemberInitExpression
                && TryRewriteEntityEquality(
                    binaryExpression.NodeType,
                    newLeft,
                    newRight,
                    equalsMethod: false,
                    out var result))
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
                var comparer = property?.GetValueComparer();

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            if (test == QueryCompilationContext.NotTranslatedExpression
                || ifTrue == QueryCompilationContext.NotTranslatedExpression
                || ifFalse == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
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
                case EntityProjectionExpression _:
                case EntityReferenceExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return new EntityReferenceExpression(entityShaperExpression);

                case ProjectionBindingExpression projectionBindingExpression
                    when projectionBindingExpression.ProjectionMember != null:
                    return ((InMemoryQueryExpression)projectionBindingExpression.QueryExpression).GetProjection(projectionBindingExpression);

                case InMemoryGroupByShaperExpression inMemoryGroupByShaperExpression:
                    return new GroupingElementExpression(
                        inMemoryGroupByShaperExpression.GroupingParameter,
                        inMemoryGroupByShaperExpression.ElementSelector,
                        inMemoryGroupByShaperExpression.ValueBufferParameter);

                default:
                    return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitInvocation(InvocationExpression invocationExpression)
            => QueryCompilationContext.NotTranslatedExpression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
            => throw new InvalidOperationException(CoreStrings.TranslationFailed(lambdaExpression.Print()));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitListInit(ListInitExpression listInitExpression)
            => QueryCompilationContext.NotTranslatedExpression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var innerExpression = Visit(memberExpression.Expression);

            // when visiting unary we remove converts from nullable to non-nullable
            // however if this happens for memberExpression.Expression we are unable to bind
            if (innerExpression != null
                && memberExpression.Expression != null
                && innerExpression.Type != memberExpression.Expression.Type
                && innerExpression.Type.IsNullableType()
                && innerExpression.Type.UnwrapNullableType() == memberExpression.Expression.Type)
            {
                innerExpression = Expression.Convert(innerExpression, memberExpression.Expression.Type);
            }

            if (memberExpression.Expression != null
                && innerExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
        {
            var expression = Visit(memberAssignment.Expression);
            if (expression == QueryCompilationContext.NotTranslatedExpression)
            {
                return memberAssignment.Update(Expression.Convert(expression, memberAssignment.Expression.Type));
            }

            if (IsConvertedToNullable(expression, memberAssignment.Expression))
            {
                expression = ConvertToNonNullable(expression);
            }

            return memberAssignment.Update(expression);
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
            if (newExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
            for (var i = 0; i < newBindings.Length; i++)
            {
                if (memberInitExpression.Bindings[i].BindingType != MemberBindingType.Assignment)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                newBindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);
                if (((MemberAssignment)newBindings[i]).Expression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert
                    && unaryExpression.Operand == QueryCompilationContext.NotTranslatedExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
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
                return TryBindMember(Visit(source), MemberIdentity.Create(propertyName), methodCallExpression.Type)
                    ?? QueryCompilationContext.NotTranslatedExpression;
            }

            // GroupBy Aggregate case
            if (methodCallExpression.Object == null
                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                && methodCallExpression.Arguments.Count > 0)
            {
                if (methodCallExpression.Arguments[0].Type.TryGetElementType(typeof(IQueryable<>)) == null
                    && Visit(methodCallExpression.Arguments[0]) is GroupingElementExpression groupingElementExpression)
                {
                    Expression? result = null;
                    switch (methodCallExpression.Method.Name)
                    {
                        case nameof(Enumerable.Average):
                        {
                            if (methodCallExpression.Arguments.Count == 2)
                            {
                                groupingElementExpression = ApplySelector(
                                    groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            }

                            var expression = ApplySelect(groupingElementExpression);

                            result = expression == null
                                ? null
                                : Expression.Call(
                                    EnumerableMethods.GetAverageWithoutSelector(expression.Type.GetSequenceType()), expression);
                            break;
                        }

                        case nameof(Enumerable.Count):
                        {
                            if (methodCallExpression.Arguments.Count == 2)
                            {
                                var temporaryGroupingElementExpression = ApplyPredicate(
                                    groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                                if (temporaryGroupingElementExpression == null)
                                {
                                    result = null;
                                    break;
                                }

                                groupingElementExpression = temporaryGroupingElementExpression;
                            }

                            var expression = ApplySelect(groupingElementExpression);

                            result = expression == null
                                ? null
                                : Expression.Call(
                                    EnumerableMethods.CountWithoutPredicate.MakeGenericMethod(expression.Type.GetSequenceType()),
                                    expression);
                            break;
                        }

                        case nameof(Enumerable.Distinct):
                            result = groupingElementExpression.Selector is EntityShaperExpression
                                ? groupingElementExpression
                                : groupingElementExpression.IsDistinct
                                    ? null
                                    : groupingElementExpression.ApplyDistinct();
                            break;

                        case nameof(Enumerable.LongCount):
                        {
                            if (methodCallExpression.Arguments.Count == 2)
                            {
                                var temporaryGroupingElementExpression = ApplyPredicate(
                                    groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                                if (temporaryGroupingElementExpression == null)
                                {
                                    result = null;
                                    break;
                                }

                                groupingElementExpression = temporaryGroupingElementExpression;
                            }

                            var expression = ApplySelect(groupingElementExpression);

                            result = expression == null
                                ? null
                                : Expression.Call(
                                    EnumerableMethods.LongCountWithoutPredicate.MakeGenericMethod(expression.Type.GetSequenceType()),
                                    expression);
                            break;
                        }

                        case nameof(Enumerable.Max):
                        {
                            if (methodCallExpression.Arguments.Count == 2)
                            {
                                groupingElementExpression = ApplySelector(
                                    groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            }

                            var expression = ApplySelect(groupingElementExpression);
                            if (expression == null
                                || expression is ParameterExpression)
                            {
                                result = null;
                            }
                            else
                            {
                                var type = expression.Type.GetSequenceType();
                                var aggregateMethod = EnumerableMethods.GetMaxWithoutSelector(type);
                                if (aggregateMethod.IsGenericMethod)
                                {
                                    aggregateMethod = aggregateMethod.MakeGenericMethod(type);
                                }

                                result = Expression.Call(aggregateMethod, expression);
                            }

                            break;
                        }

                        case nameof(Enumerable.Min):
                        {
                            if (methodCallExpression.Arguments.Count == 2)
                            {
                                groupingElementExpression = ApplySelector(
                                    groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            }

                            var expression = ApplySelect(groupingElementExpression);
                            if (expression == null
                                || expression is ParameterExpression)
                            {
                                result = null;
                            }
                            else
                            {
                                var type = expression.Type.GetSequenceType();
                                var aggregateMethod = EnumerableMethods.GetMinWithoutSelector(type);
                                if (aggregateMethod.IsGenericMethod)
                                {
                                    aggregateMethod = aggregateMethod.MakeGenericMethod(type);
                                }

                                result = Expression.Call(aggregateMethod, expression);
                            }

                            break;
                        }

                        case nameof(Enumerable.Select):
                            result = ApplySelector(groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            break;

                        case nameof(Enumerable.Sum):
                        {
                            if (methodCallExpression.Arguments.Count == 2)
                            {
                                groupingElementExpression = ApplySelector(
                                    groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            }

                            var expression = ApplySelect(groupingElementExpression);

                            result = expression == null
                                ? null
                                : Expression.Call(
                                    EnumerableMethods.GetSumWithoutSelector(expression.Type.GetSequenceType()), expression);
                            break;
                        }

                        case nameof(Enumerable.Where):
                            result = ApplyPredicate(groupingElementExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            break;

                        default:
                            result = null;
                            break;
                    }

                    return result ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));

                    GroupingElementExpression? ApplyPredicate(GroupingElementExpression groupingElement, LambdaExpression lambdaExpression)
                    {
                        var predicate = TranslateInternal(RemapLambda(groupingElement, lambdaExpression));

                        if (predicate == null)
                        {
                            return null;
                        }

                        if (predicate.Type != typeof(bool))
                        {
                            predicate = Expression.Equal(predicate, Expression.Constant(true, typeof(bool?)));
                        }

                        return groupingElement.UpdateSource(
                            Expression.Call(
                                EnumerableMethods.Where.MakeGenericMethod(typeof(ValueBuffer)),
                                groupingElement.Source,
                                Expression.Lambda(predicate, groupingElement.ValueBufferParameter)));
                    }

                    Expression? ApplySelect(GroupingElementExpression groupingElement)
                    {
                        var selector = TranslateInternal(groupingElement.Selector);

                        if (selector == null)
                        {
                            return groupingElement.Selector is EntityShaperExpression
                                ? groupingElement.Source
                                : null;
                        }

                        var result = Expression.Call(
                            EnumerableMethods.Select.MakeGenericMethod(typeof(ValueBuffer), selector.Type),
                            groupingElement.Source,
                            Expression.Lambda(selector, groupingElement.ValueBufferParameter));

                        if (groupingElement.IsDistinct)
                        {
                            result = Expression.Call(
                                EnumerableMethods.Distinct.MakeGenericMethod(selector.Type),
                                result);
                        }

                        return result;
                    }

                    static GroupingElementExpression ApplySelector(
                        GroupingElementExpression groupingElement,
                        LambdaExpression lambdaExpression)
                    {
                        var selector = RemapLambda(groupingElement, lambdaExpression);

                        return groupingElement.ApplySelector(selector);
                    }

                    static Expression RemapLambda(GroupingElementExpression groupingElement, LambdaExpression lambdaExpression)
                        => ReplacingExpressionVisitor.Replace(
                            lambdaExpression.Parameters[0], groupingElement.Selector, lambdaExpression.Body);
                }
            }

            // Subquery case
            var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
            if (subqueryTranslation != null)
            {
                var subquery = (InMemoryQueryExpression)subqueryTranslation.QueryExpression;
                if (subqueryTranslation.ResultCardinality == ResultCardinality.Enumerable)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                var shaperExpression = subqueryTranslation.ShaperExpression;
                var innerExpression = shaperExpression;
                Type? convertedType = null;
                if (shaperExpression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert)
                {
                    convertedType = unaryExpression.Type;
                    innerExpression = unaryExpression.Operand;
                }

                if (innerExpression is EntityShaperExpression entityShaperExpression
                    && (convertedType == null
                        || convertedType.IsAssignableFrom(entityShaperExpression.Type)))
                {
                    return new EntityReferenceExpression(subqueryTranslation.UpdateShaperExpression(innerExpression));
                }

                if (!(innerExpression is ProjectionBindingExpression projectionBindingExpression
                    && (convertedType == null
                        || convertedType.MakeNullable() == innerExpression.Type)))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (projectionBindingExpression.ProjectionMember == null)
                {
                    // We don't lift scalar subquery with client eval
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                return ProcessSingleResultScalar(
                    subquery,
                    subquery.GetProjection(projectionBindingExpression),
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
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    visitedArguments[i - 1] = argument;
                }

                return Expression.Call(_inMemoryLikeMethodInfo, visitedArguments);
            }

            if (methodCallExpression.Method == _randomMethodInfo)
            {
                return Expression.Call(Expression.New(typeof(Random)), _randomNextDoubleMethodInfo);
            }

            Expression? @object = null;
            Expression[] arguments;
            var method = methodCallExpression.Method;

            if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object != null
                && methodCallExpression.Arguments.Count == 1)
            {
                var left = Visit(methodCallExpression.Object);
                var right = Visit(methodCallExpression.Arguments[0])!;

                if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Object : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : right,
                    equalsMethod: true,
                    out var result))
                {
                    return result;
                }

                if (TranslationFailed(methodCallExpression.Object, left)
                    || TranslationFailed(methodCallExpression.Arguments[0], right))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                @object = left;
                arguments = new Expression[1] { right };
            }
            else if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object == null
                && methodCallExpression.Arguments.Count == 2)
            {
                if (methodCallExpression.Arguments[0].Type == typeof(object[])
                    && methodCallExpression.Arguments[0] is NewArrayExpression)
                {
                    return Visit(
                        ConvertObjectArrayEqualityComparison(
                            methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]));
                }

                var left = Visit(methodCallExpression.Arguments[0])!;
                var right = Visit(methodCallExpression.Arguments[1])!;

                if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[1] : right,
                    equalsMethod: true,
                    out var result))
                {
                    return result;
                }

                if (TranslationFailed(methodCallExpression.Arguments[0], left)
                    || TranslationFailed(methodCallExpression.Arguments[1], right))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                arguments = new Expression[2] { left, right };
            }
            else if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
            {
                var enumerable = Visit(methodCallExpression.Arguments[0])!;
                var item = Visit(methodCallExpression.Arguments[1])!;

                if (TryRewriteContainsEntity(enumerable,
                    item == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[1] : item,
                    out var result))
                {
                    return result;
                }

                if (TranslationFailed(methodCallExpression.Arguments[0], enumerable)
                    || TranslationFailed(methodCallExpression.Arguments[1], item))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                arguments = new Expression[2] { enumerable, item };
            }
            else if (methodCallExpression.Arguments.Count == 1
                && method.IsContainsMethod())
            {
                var enumerable = Visit(methodCallExpression.Object);
                var item = Visit(methodCallExpression.Arguments[0])!;

                if (TryRewriteContainsEntity(enumerable,
                    item == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : item,
                    out var result))
                {
                    return result;
                }

                if (TranslationFailed(methodCallExpression.Object, enumerable)
                    || TranslationFailed(methodCallExpression.Arguments[0], item))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                @object = enumerable;
                arguments = new Expression[1] { item };
            }
            else
            {
                @object = Visit(methodCallExpression.Object);
                if (TranslationFailed(methodCallExpression.Object, @object))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                arguments = new Expression[methodCallExpression.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = Visit(methodCallExpression.Arguments[i]);
                    if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                    {
                        return QueryCompilationContext.NotTranslatedExpression;
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
                && @object!.Type.IsNullableType()
                && methodCallExpression.Method.Name != nameof(Nullable<int>.GetValueOrDefault))
            {
                var result = (Expression)methodCallExpression.Update(
                    Expression.Convert(@object, methodCallExpression.Object.Type),
                    arguments);

                result = ConvertToNullable(result);
                var objectNullCheck = Expression.Equal(@object, Expression.Constant(null, @object.Type));
                // instance.Equals(argument) should translate to
                // instance == null ? argument == null : instance.Equals(argument)
                if (method.Name == nameof(object.Equals))
                {
                    var argument = arguments[0];
                    if (argument.NodeType == ExpressionType.Convert
                        && argument is UnaryExpression unaryExpression
                        && argument.Type == unaryExpression.Operand.Type.UnwrapNullableType())
                    {
                        argument = unaryExpression.Operand;
                    }

                    if (!argument.Type.IsNullableType())
                    {
                        argument = Expression.Convert(argument, argument.Type.MakeNullable());
                    }

                    return Expression.Condition(
                        objectNullCheck,
                        ConvertToNullable(Expression.Equal(argument, Expression.Constant(null, argument.Type))),
                        result);
                }

                return Expression.Condition(objectNullCheck, Expression.Constant(null, result.Type), result);
            }

            // TODO-Nullable bug
            return methodCallExpression.Update(@object!, arguments);
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

            var newArguments = new List<Expression>();
            foreach (var argument in newExpression.Arguments)
            {
                var newArgument = Visit(argument);
                if (newArgument == QueryCompilationContext.NotTranslatedExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (IsConvertedToNullable(newArgument, argument))
                {
                    newArgument = ConvertToNonNullable(newArgument);
                }

                newArguments.Add(newArgument);
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
        {
            Check.NotNull(newArrayExpression, nameof(newArrayExpression));

            var newExpressions = new List<Expression>();
            foreach (var expression in newArrayExpression.Expressions)
            {
                var newExpression = Visit(expression);
                if (newExpression == QueryCompilationContext.NotTranslatedExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (IsConvertedToNullable(newExpression, expression))
                {
                    newExpression = ConvertToNonNullable(newExpression);
                }

                newExpressions.Add(newExpression);
            }

            return newArrayExpression.Update(newExpressions);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
                    // All hierarchies have discriminator property
                    var discriminatorProperty = entityType.FindDiscriminatorProperty()!;
                    var boundProperty = BindProperty(entityReferenceExpression, discriminatorProperty, discriminatorProperty.ClrType);
                    // KeyValueComparer is not null at runtime
                    var valueComparer = discriminatorProperty.GetKeyValueComparer();

                    var equals = valueComparer.ExtractEqualsBody(
                        boundProperty!,
                        Expression.Constant(derivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType));

                    foreach (var derivedDerivedType in derivedType.GetDerivedTypes())
                    {
                        equals = Expression.OrElse(
                            equals,
                            valueComparer.ExtractEqualsBody(
                                boundProperty!,
                                Expression.Constant(derivedDerivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType)));
                    }

                    return equals;
                }
            }

            return QueryCompilationContext.NotTranslatedExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            var newOperand = Visit(unaryExpression.Operand);
            if (newOperand == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
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

        private Expression? TryBindMember(Expression? source, MemberIdentity member, Type type)
        {
            if (!(source is EntityReferenceExpression entityReferenceExpression))
            {
                return null;
            }

            var entityType = entityReferenceExpression.EntityType;

            var property = member.MemberInfo != null
                ? entityType.FindProperty(member.MemberInfo)
                : entityType.FindProperty(member.Name!);

            if (property != null)
            {
                return BindProperty(entityReferenceExpression, property, type);
            }

            AddTranslationErrorDetails(
                CoreStrings.QueryUnableToTranslateMember(
                    member.Name,
                    entityReferenceExpression.EntityType.DisplayName()));

            return null;
        }

        private Expression? BindProperty(EntityReferenceExpression entityReferenceExpression, IProperty property, Type type)
        {
            if (entityReferenceExpression.ParameterEntity != null)
            {
                var valueBufferExpression = Visit(entityReferenceExpression.ParameterEntity.ValueBufferExpression);
                if (valueBufferExpression == QueryCompilationContext.NotTranslatedExpression)
                {
                    return null;
                }

                var result = ((EntityProjectionExpression)valueBufferExpression).BindProperty(property);

                // if the result type change was just nullability change e.g from int to int?
                // we want to preserve the new type for null propagation
                return result.Type != type
                    && !(result.Type.IsNullableType()
                        && !type.IsNullableType()
                        && result.Type.UnwrapNullableType() == type)
                    ? Expression.Convert(result, type)
                    : (Expression)result;
            }

            if (entityReferenceExpression.SubqueryEntity != null)
            {
                var entityShaper = (EntityShaperExpression)entityReferenceExpression.SubqueryEntity.ShaperExpression;
                var inMemoryQueryExpression = (InMemoryQueryExpression)entityReferenceExpression.SubqueryEntity.QueryExpression;

                Expression readValueExpression;
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaper.ValueBufferExpression;
                var entityProjectionExpression = (EntityProjectionExpression)inMemoryQueryExpression.GetProjection(
                    projectionBindingExpression);
                readValueExpression = entityProjectionExpression.BindProperty(property);

                return ProcessSingleResultScalar(
                    inMemoryQueryExpression,
                    readValueExpression,
                    type);
            }

            return null;
        }

        private static Expression ProcessSingleResultScalar(
            InMemoryQueryExpression inMemoryQueryExpression,
            Expression readValueExpression,
            Type type)
        {
            if (inMemoryQueryExpression.ServerQueryExpression is not NewExpression)
            {
                // The terminating operator is not applied
                // It is of FirstOrDefault kind
                // So we change to single column projection and then apply it.
                inMemoryQueryExpression.ReplaceProjection(new Dictionary<ProjectionMember, Expression>
                {
                    { new ProjectionMember(), readValueExpression }
                });
                inMemoryQueryExpression.ApplyProjection();
            }

            var serverQuery = inMemoryQueryExpression.ServerQueryExpression;
            serverQuery = ((LambdaExpression)((NewExpression)serverQuery).Arguments[0]).Body;
            if (serverQuery is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Type == typeof(object))
            {
                serverQuery = unaryExpression.Operand;
            }

            var valueBufferVariable = Expression.Variable(typeof(ValueBuffer));
            var readExpression = valueBufferVariable.CreateValueBufferReadValueExpression(type, index: 0, property: null);
            return Expression.Block(
                variables: new[] { valueBufferVariable },
                Expression.Assign(valueBufferVariable, serverQuery),
                Expression.Condition(
                    Expression.MakeMemberAccess(valueBufferVariable, _valueBufferIsEmpty),
                    Expression.Default(type),
                    readExpression));
        }

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
            => (T)queryContext.ParameterValues[parameterName]!;

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

        private IProperty? FindProperty(Expression expression)
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
                return readValueMethodCall.Arguments[2].GetConstantValue<IProperty>();
            }

            return null;
        }

        private bool TryRewriteContainsEntity(Expression? source, Expression item, [NotNullWhen(true)] out Expression? result)
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
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nameof(Queryable.Contains), entityType.DisplayName()));
            }

            if (primaryKeyProperties.Count > 1)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported(nameof(Queryable.Contains), entityType.DisplayName()));
            }

            var property = primaryKeyProperties[0];
            Expression rewrittenSource;
            switch (source)
            {
                case ConstantExpression constantExpression:
                    var values = constantExpression.GetConstantValue<IEnumerable>();
                    var propertyValueList =
                        (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.ClrType.MakeNullable()))!;
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
                    var parameterName = methodCallExpression.Arguments[1].GetConstantValue<string>();
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterListValueExtractor.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(parameterName, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter
                    );

                    var newParameterName =
                        $"{_runtimeParameterPrefix}"
                        + $"{parameterName[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

                    rewrittenSource = _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                    break;

                default:
                    return false;
            }

            result = Visit(
                Expression.Call(
                    EnumerableMethods.Contains.MakeGenericMethod(property.ClrType.MakeNullable()),
                    rewrittenSource,
                    CreatePropertyAccessExpression(item, property)));

            return true;
        }

        private bool TryRewriteEntityEquality(
            ExpressionType nodeType, Expression left, Expression right, bool equalsMethod, [NotNullWhen(true)] out Expression? result)
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
                var nonNullEntityReference = (IsNullConstantExpression(left) ? rightEntityReference : leftEntityReference)!;
                var entityType1 = nonNullEntityReference.EntityType;
                var primaryKeyProperties1 = entityType1.FindPrimaryKey()?.Properties;
                if (primaryKeyProperties1 == null)
                {
                    throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                        nodeType == ExpressionType.Equal
                            ? equalsMethod ? nameof(object.Equals) : "=="
                            : equalsMethod ? "!" + nameof(object.Equals) : "!=",
                        entityType1.DisplayName()));
                }

                result = Visit(
                    primaryKeyProperties1.Select(
                            p =>
                                Expression.MakeBinary(
                                    nodeType, CreatePropertyAccessExpression(nonNullEntityReference, p),
                                    Expression.Constant(null, p.ClrType.MakeNullable())))
                        .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

                return true;
            }

            var leftEntityType = leftEntityReference?.EntityType;
            var rightEntityType = rightEntityReference?.EntityType;
            var entityType = leftEntityType ?? rightEntityType;

            Check.DebugAssert(entityType != null, "At least either side should be entityReference so entityType should be non-null.");

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
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nodeType == ExpressionType.Equal
                        ? equalsMethod ? nameof(object.Equals) : "=="
                        : equalsMethod ? "!" + nameof(object.Equals) : "!=",
                    entityType.DisplayName()));
            }

            if (primaryKeyProperties.Count > 1
                && (leftEntityReference?.SubqueryEntity != null
                    || rightEntityReference?.SubqueryEntity != null))
            {
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported(
                    nodeType == ExpressionType.Equal
                        ? equalsMethod ? nameof(object.Equals) : "=="
                        : equalsMethod ? "!" + nameof(object.Equals) : "!=",
                    entityType.DisplayName()));
            }

            result = Visit(
                primaryKeyProperties.Select(
                        p =>
                            Expression.MakeBinary(
                                nodeType,
                                CreatePropertyAccessExpression(left, p),
                                CreatePropertyAccessExpression(right, p)))
                    .Aggregate((l, r) => nodeType == ExpressionType.Equal
                        ? Expression.AndAlso(l, r)
                        : Expression.OrElse(l, r)));

            return true;
        }

        private Expression CreatePropertyAccessExpression(Expression target, IProperty property)
        {
            switch (target)
            {
                case ConstantExpression constantExpression:
                    return Expression.Constant(
                        constantExpression.Value is null
                            ? null
                            : property.GetGetter().GetClrValue(constantExpression.Value),
                        property.ClrType.MakeNullable());

                case MethodCallExpression methodCallExpression
                    when methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == _getParameterValueMethodInfo:
                    var parameterName = methodCallExpression.Arguments[1].GetConstantValue<string>();
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterValueExtractor.MakeGenericMethod(property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(parameterName, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter);

                    var newParameterName =
                        $"{_runtimeParameterPrefix}"
                        + $"{parameterName[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

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

        private static T? ParameterValueExtractor<T>(QueryContext context, string baseParameterName, IProperty property)
        {
            var baseParameter = context.ParameterValues[baseParameterName];
            return baseParameter == null ? (T?)(object?)null : (T?)property.GetGetter().GetClrValue(baseParameter);
        }

        private static List<TProperty?>? ParameterListValueExtractor<TEntity, TProperty>(
            QueryContext context,
            string baseParameterName,
            IProperty property)
        {
            if (!(context.ParameterValues[baseParameterName] is IEnumerable<TEntity> baseListParameter))
            {
                return null;
            }

            var getter = property.GetGetter();
            return baseListParameter.Select(e => e != null ? (TProperty?)getter.GetClrValue(e) : (TProperty?)(object?)null).ToList();
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

        private static Expression ConvertObjectArrayEqualityComparison(Expression left, Expression right)
        {
            var leftExpressions = ((NewArrayExpression)left).Expressions;
            var rightExpressions = ((NewArrayExpression)right).Expressions;

            return leftExpressions.Zip(
                    rightExpressions,
                    (l, r) =>
                    {
                        l = RemoveObjectConvert(l);
                        r = RemoveObjectConvert(r);
                        if (l.Type.IsNullableType())
                        {
                            r = r.Type.IsNullableType() ? r : Expression.Convert(r, l.Type);
                        }
                        else if (r.Type.IsNullableType())
                        {
                            l = l.Type.IsNullableType() ? l : Expression.Convert(l, r.Type);
                        }

                        return Expression.Equal(l, r);
                    })
                .Aggregate((a, b) => Expression.AndAlso(a, b));

            static Expression RemoveObjectConvert(Expression expression)
                => expression is UnaryExpression unaryExpression
                    && expression.Type == typeof(object)
                    && expression.NodeType == ExpressionType.Convert
                        ? unaryExpression.Operand
                        : expression;
        }

        private static bool IsNullConstantExpression(Expression expression)
            => expression is ConstantExpression constantExpression && constantExpression.Value == null;

        [DebuggerStepThrough]
        private static bool TranslationFailed(Expression? original, Expression? translation)
            => original != null
            && (translation == QueryCompilationContext.NotTranslatedExpression || translation is EntityReferenceExpression);

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

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
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

            public EntityShaperExpression? ParameterEntity { get; }
            public ShapedQueryExpression? SubqueryEntity { get; }
            public IEntityType EntityType { get; }

            public override Type Type
                => EntityType.ClrType;

            public override ExpressionType NodeType
                => ExpressionType.Extension;

            public Expression Convert(Type type)
            {
                if (type == typeof(object) // Ignore object conversion
                    || type.IsAssignableFrom(Type)) // Ignore casting to base type/interface
                {
                    return this;
                }

                var derivedEntityType = EntityType.GetDerivedTypes().FirstOrDefault(et => et.ClrType == type);

                return derivedEntityType == null
                    ? QueryCompilationContext.NotTranslatedExpression
                    : new EntityReferenceExpression(this, derivedEntityType);
            }
        }

        private sealed class GroupingElementExpression : Expression
        {
            public GroupingElementExpression(Expression source, Expression selector, ParameterExpression valueBufferParameter)
            {
                Source = source;
                ValueBufferParameter = valueBufferParameter;
                Selector = selector;
            }

            public Expression Source { get; private set; }
            public bool IsDistinct { get; private set; }
            public Expression Selector { get; private set; }
            public ParameterExpression ValueBufferParameter { get; }

            public GroupingElementExpression ApplyDistinct()
            {
                IsDistinct = true;

                return this;
            }

            public GroupingElementExpression ApplySelector(Expression expression)
            {
                Selector = expression;

                return this;
            }

            public GroupingElementExpression UpdateSource(Expression source)
            {
                Source = source;

                return this;
            }

            public override Type Type
                => typeof(IEnumerable<>).MakeGenericType(Selector.Type);

            public override ExpressionType NodeType
                => ExpressionType.Extension;
        }
    }
}
