// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.Data.Core.Resources;

    [DebuggerStepThrough]
    internal static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyAccess(this LambdaExpression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyInfo
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchSimplePropertyAccess(propertyAccessExpression.Body);

            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    Strings.InvalidPropertyExpression(propertyAccessExpression),
                    "propertyAccessExpression");
            }

            return propertyInfo;
        }

        public static IList<PropertyInfo> GetPropertyAccessList(this LambdaExpression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPaths
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null)
            {
                throw new ArgumentException(
                    Strings.InvalidPropertiesExpression(propertyAccessExpression),
                    "propertyAccessExpression");
            }

            return propertyPaths;
        }

        private static IList<PropertyInfo> MatchPropertyAccessList(
            this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyInfo> propertyMatcher)
        {
            DebugCheck.NotNull(lambdaExpression);
            DebugCheck.NotNull(propertyMatcher);
            Debug.Assert(lambdaExpression.Body != null);

            var newExpression
                = RemoveConvert(lambdaExpression.Body) as NewExpression;

            if (newExpression != null)
            {
                var parameterExpression
                    = lambdaExpression.Parameters.Single();

                var propertyInfos
                    = newExpression
                        .Arguments
                        .Select(a => propertyMatcher(a, parameterExpression))
                        .Where(p => p != null)
                        .ToList();

                if (propertyInfos.Count != newExpression.Arguments.Count)
                {
                    return null;
                }

                return propertyInfos;
            }

            var propertyPath
                = propertyMatcher(lambdaExpression.Body, lambdaExpression.Parameters.Single());

            return (propertyPath != null) ? new[] { propertyPath } : null;
        }

        private static PropertyInfo MatchSimplePropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            DebugCheck.NotNull(propertyAccessExpression);

            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyInfos != null && propertyInfos.Length == 1 ? propertyInfos[0] : null;
        }

        private static PropertyInfo[] MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            DebugCheck.NotNull(parameterExpression);
            DebugCheck.NotNull(propertyAccessExpression);

            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveConvert(propertyAccessExpression) as MemberExpression;

                if (memberExpression == null)
                {
                    return null;
                }

                var propertyInfo = memberExpression.Member as PropertyInfo;

                if (propertyInfo == null)
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (memberExpression.Expression != parameterExpression);

            return propertyInfos.ToArray();
        }

        private static Expression RemoveConvert(this Expression expression)
        {
            DebugCheck.NotNull(expression);

            while ((expression != null)
                   && (expression.NodeType == ExpressionType.Convert
                       || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }
    }
}
