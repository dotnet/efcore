// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SubqueryMemberPushdownExpressionVisitor : ExpressionVisitor
    {
        private static readonly List<MethodInfo> _supportedMethods = new()
        {
            QueryableMethods.FirstWithPredicate,
            QueryableMethods.FirstWithoutPredicate,
            QueryableMethods.FirstOrDefaultWithPredicate,
            QueryableMethods.FirstOrDefaultWithoutPredicate,
            QueryableMethods.SingleWithPredicate,
            QueryableMethods.SingleWithoutPredicate,
            QueryableMethods.SingleOrDefaultWithPredicate,
            QueryableMethods.SingleOrDefaultWithoutPredicate,
            QueryableMethods.LastWithPredicate,
            QueryableMethods.LastWithoutPredicate,
            QueryableMethods.LastOrDefaultWithPredicate,
            QueryableMethods.LastOrDefaultWithoutPredicate
            //QueryableMethodProvider.ElementAtMethodInfo,
            //QueryableMethodProvider.ElementAtOrDefaultMethodInfo
        };

        private static readonly IDictionary<MethodInfo, MethodInfo> _predicateLessMethodInfo = new Dictionary<MethodInfo, MethodInfo>
        {
            { QueryableMethods.FirstWithPredicate, QueryableMethods.FirstWithoutPredicate },
            { QueryableMethods.FirstOrDefaultWithPredicate, QueryableMethods.FirstOrDefaultWithoutPredicate },
            { QueryableMethods.SingleWithPredicate, QueryableMethods.SingleWithoutPredicate },
            { QueryableMethods.SingleOrDefaultWithPredicate, QueryableMethods.SingleOrDefaultWithoutPredicate },
            { QueryableMethods.LastWithPredicate, QueryableMethods.LastWithoutPredicate },
            { QueryableMethods.LastOrDefaultWithPredicate, QueryableMethods.LastOrDefaultWithoutPredicate }
        };

        private readonly IModel _model;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SubqueryMemberPushdownExpressionVisitor(IModel model)
        {
            _model = model;
        }

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
            if (innerExpression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsGenericMethod
                && _supportedMethods.Contains(methodCallExpression.Method.GetGenericMethodDefinition()))
            {
                return PushdownMember(
                    methodCallExpression,
                    (target, nullable) =>
                    {
                        var memberAccessExpression = Expression.MakeMemberAccess(target, memberExpression.Member);

                        return nullable && !memberAccessExpression.Type.IsNullableType()
                            ? Expression.Convert(memberAccessExpression, memberAccessExpression.Type.MakeNullable())
                            : (Expression)memberAccessExpression;
                    },
                    memberExpression.Type);
            }

            return memberExpression.Update(innerExpression);
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

            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out _))
            {
                source = Visit(source);

                if (source is MethodCallExpression innerMethodCall
                    && innerMethodCall.Method.IsGenericMethod
                    && _supportedMethods.Contains(innerMethodCall.Method.GetGenericMethodDefinition()))
                {
                    return PushdownMember(
                        innerMethodCall,
                        (target, nullable) =>
                        {
                            var propertyType = methodCallExpression.Type;
                            if (nullable && !propertyType.IsNullableType())
                            {
                                propertyType = propertyType.MakeNullable();
                            }

                            return Expression.Call(
                                EF.PropertyMethod.MakeGenericMethod(propertyType),
                                target,
                                methodCallExpression.Arguments[1]);
                        },
                        methodCallExpression.Type);
                }
            }

            if (methodCallExpression.TryGetIndexerArguments(_model, out source, out _))
            {
                source = Visit(source);

                if (source is MethodCallExpression innerMethodCall
                    && innerMethodCall.Method.IsGenericMethod
                    && _supportedMethods.Contains(innerMethodCall.Method.GetGenericMethodDefinition()))
                {
                    return PushdownMember(
                        innerMethodCall,
                        (target, nullable) =>
                        {
                            var propertyType = methodCallExpression.Type;
                            if (nullable && !propertyType.IsNullableType())
                            {
                                propertyType = propertyType.MakeNullable();
                            }

                            var indexerExpression = Expression.Call(
                                target,
                                methodCallExpression.Method, methodCallExpression.Arguments[0]);

                            return nullable && !indexerExpression.Type.IsNullableType()
                                ? Expression.Convert(indexerExpression, indexerExpression.Type.MakeNullable())
                                : (Expression)indexerExpression;
                        },
                        methodCallExpression.Type);
                }
            }

            // Avoid pushing down a collection navigation which is followed by AsQueryable
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable
                && methodCallExpression.Arguments[0] is MemberExpression memberExpression)
            {
                var updatedMemberExpression = memberExpression.Update(Visit(memberExpression.Expression));

                return Expression.Call(
                    QueryableMethods.AsQueryable.MakeGenericMethod(updatedMemberExpression.Type.GetSequenceType()),
                    updatedMemberExpression);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Expression PushdownMember(
            MethodCallExpression methodCallExpression,
            Func<Expression, bool, Expression> createSelector,
            Type returnType)
        {
            var source = methodCallExpression.Arguments[0];
            var queryableType = source.Type.GetSequenceType();
            var genericMethod = methodCallExpression.Method.GetGenericMethodDefinition();
            if (methodCallExpression.Arguments.Count == 2)
            {
                // Move predicate to Where so that we can change shape before operator
                source = Expression.Call(
                    QueryableMethods.Where.MakeGenericMethod(queryableType),
                    source,
                    methodCallExpression.Arguments[1]);

                genericMethod = _predicateLessMethodInfo[genericMethod];
            }

            if (source is MethodCallExpression sourceMethodCallExpression
                && sourceMethodCallExpression.Method.IsGenericMethod
                && sourceMethodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.Select)
            {
                var selector = sourceMethodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                var selectorBody = selector.Body;
                var memberAccessExpression = createSelector(
                    selectorBody, methodCallExpression.Method.Name.EndsWith("OrDefault", StringComparison.Ordinal));

                source = Expression.Call(
                    QueryableMethods.Select.MakeGenericMethod(
                        sourceMethodCallExpression.Arguments[0].Type.GetSequenceType(), memberAccessExpression.Type),
                    sourceMethodCallExpression.Arguments[0],
                    Expression.Quote(Expression.Lambda(memberAccessExpression, selector.Parameters[0])));

                source = Visit(source);
            }
            else
            {
                var parameter = Expression.Parameter(queryableType, "s");
                var memberAccessExpression = createSelector(
                    parameter, methodCallExpression.Method.Name.EndsWith("OrDefault", StringComparison.Ordinal));

                source = Expression.Call(
                    QueryableMethods.Select.MakeGenericMethod(queryableType, memberAccessExpression.Type),
                    source,
                    Expression.Quote(Expression.Lambda(memberAccessExpression, parameter)));
            }

            source = Expression.Call(genericMethod.MakeGenericMethod(source.Type.GetSequenceType()), source);

            return source.Type != returnType
                ? Expression.Convert(source, returnType)
                : source;
        }
    }
}
