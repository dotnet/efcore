// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsNullConstantExpression([NotNull] this Expression expression)
            => RemoveConvert(expression) is ConstantExpression constantExpression
                && constantExpression.Value == null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<TMemberInfo> MatchMemberAccessList<TMemberInfo>(
            [NotNull] this LambdaExpression lambdaExpression, [NotNull] Func<Expression, Expression, TMemberInfo> memberMatcher)
            where TMemberInfo : MemberInfo
        {
            Check.DebugAssert(lambdaExpression.Body != null, "lambdaExpression.Body is null");
            Check.DebugAssert(lambdaExpression.Parameters.Count == 1, "lambdaExpression.Parameters.Count is " + lambdaExpression.Parameters.Count + ". Should be 1.");

            var parameterExpression = lambdaExpression.Parameters[0];

            if (RemoveConvert(lambdaExpression.Body) is NewExpression newExpression)
            {
                var memberInfos
                    = newExpression
                        .Arguments
                        .Select(a => memberMatcher(a, parameterExpression))
                        .Where(p => p != null)
                        .ToList();

                return memberInfos.Count != newExpression.Arguments.Count ? null : memberInfos;
            }

            var memberPath = memberMatcher(lambdaExpression.Body, parameterExpression);

            return memberPath != null ? new[] { memberPath } : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static TMemberInfo MatchSimpleMemberAccess<TMemberInfo>(
            [NotNull] this Expression parameterExpression, [NotNull] Expression memberAccessExpression)
            where TMemberInfo : MemberInfo
        {
            var memberInfos = MatchMemberAccess<TMemberInfo>(parameterExpression, memberAccessExpression);

            return memberInfos?.Count == 1 ? memberInfos[0] as TMemberInfo : null;
        }

        private static IReadOnlyList<TMemberInfo> MatchMemberAccess<TMemberInfo>(
            this Expression parameterExpression, Expression memberAccessExpression)
            where TMemberInfo : MemberInfo
        {
            var memberInfos = new List<TMemberInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveTypeAs(RemoveConvert(memberAccessExpression)) as MemberExpression;

                if (!(memberExpression?.Member is TMemberInfo memberInfo))
                {
                    return null;
                }

                memberInfos.Insert(0, memberInfo);

                memberAccessExpression = memberExpression.Expression;
            }
            while (RemoveTypeAs(RemoveConvert(memberExpression.Expression)) != parameterExpression);

            return memberInfos;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Expression RemoveTypeAs([CanBeNull] this Expression expression)
        {
            while (expression?.NodeType == ExpressionType.TypeAs)
            {
                expression = ((UnaryExpression)RemoveConvert(expression)).Operand;
            }

            return expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsLogicalOperation([NotNull] this Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.OrElse;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static LambdaExpression GetLambdaOrNull([NotNull] this Expression expression)
            => expression is LambdaExpression lambda
                ? lambda
                : expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
                    ? (LambdaExpression)unary.Operand
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsLogicalNot([NotNull] this UnaryExpression sqlUnaryExpression)
            => sqlUnaryExpression.NodeType == ExpressionType.Not
                && (sqlUnaryExpression.Type == typeof(bool)
                    || sqlUnaryExpression.Type == typeof(bool?));

        private static Expression RemoveConvert(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression
                && (expression.NodeType == ExpressionType.Convert
                    || expression.NodeType == ExpressionType.ConvertChecked))
            {
                return RemoveConvert(unaryExpression.Operand);
            }

            return expression;
        }
    }
}
