// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ExpectedQueryRewritingVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _maybeMethod
            = typeof(QueryTestExtensions).GetMethod(nameof(QueryTestExtensions.Maybe));

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (!memberExpression.Type.IsValueType
                && !memberExpression.Type.IsNullableValueType()
                && memberExpression.Expression != null)
            { 
                var expression = Visit(memberExpression.Expression);

                var lambdaParameter = Expression.Parameter(expression.Type, "x");
                var lambda = Expression.Lambda(memberExpression.Update(lambdaParameter), lambdaParameter);
                var method = _maybeMethod.MakeGenericMethod(expression.Type, memberExpression.Type);

                return Expression.Call(method, expression, lambda);
            }

            return base.VisitMember(memberExpression);
        }
    }
}
