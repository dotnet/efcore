// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity;

// ReSharper disable once CheckNamespace

namespace System.Linq.Expressions
{
    [DebuggerStepThrough]
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyAccess([NotNull] this LambdaExpression propertyAccessExpression)
        {
            Contract.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyInfo
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchSimplePropertyAccess(propertyAccessExpression.Body);

            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    Strings.FormatInvalidPropertyExpression(propertyAccessExpression),
                    "propertyAccessExpression");
            }

            return propertyInfo;
        }

        public static IList<PropertyInfo> GetPropertyAccessList([NotNull] this LambdaExpression propertyAccessExpression)
        {
            Contract.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPaths
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null)
            {
                throw new ArgumentException(
                    Strings.FormatInvalidPropertiesExpression(propertyAccessExpression),
                    "propertyAccessExpression");
            }

            return propertyPaths;
        }

        private static IList<PropertyInfo> MatchPropertyAccessList(
            this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyInfo> propertyMatcher)
        {
            Contract.Assert(lambdaExpression.Body != null);

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
            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyInfos != null && propertyInfos.Length == 1 ? propertyInfos[0] : null;
        }

        private static PropertyInfo[] MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
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
