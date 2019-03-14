// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public static class ExpressionExtensions
    {
        public static LambdaExpression UnwrapQuote(this Expression expression)
            => expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
            ? (LambdaExpression)unary.Operand
            : (LambdaExpression)expression;

        public static bool IsIncludeMethod(this MethodCallExpression methodCallExpression)
            => methodCallExpression.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && methodCallExpression.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include);

        public static Expression BuildPropertyAccess(this Expression root, List<string> path)
        {
            var result = root;
            foreach (var pathElement in path)
            {
                result = Expression.PropertyOrField(result, pathElement);
            }

            return result;
        }

        public static Expression CombineAndRemap(
            Expression source,
            ParameterExpression sourceParameter,
            Expression replaceWith)
            => new ExpressionCombiningVisitor(sourceParameter, replaceWith).Visit(source);

        public class ExpressionCombiningVisitor : ExpressionVisitor
        {
            private ParameterExpression _sourceParameter;
            private Expression _replaceWith;

            public ExpressionCombiningVisitor(
                ParameterExpression sourceParameter,
                Expression replaceWith)
            {
                _sourceParameter = sourceParameter;
                _replaceWith = replaceWith;
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
                => parameterExpression == _sourceParameter
                ? _replaceWith
                : base.VisitParameter(parameterExpression);

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                var newSource = Visit(memberExpression.Expression);
                if (newSource is NewExpression newExpression)
                {
                    var matchingMemberIndex = newExpression.Members.Select((m, i) => new { index = i, match = m == memberExpression.Member }).Where(r => r.match).SingleOrDefault()?.index;
                    if (matchingMemberIndex.HasValue)
                    {
                        return newExpression.Arguments[matchingMemberIndex.Value];
                    }
                }

                return newSource != memberExpression.Expression
                    ? memberExpression.Update(newSource)
                    : memberExpression;
            }
        }
    }

}
