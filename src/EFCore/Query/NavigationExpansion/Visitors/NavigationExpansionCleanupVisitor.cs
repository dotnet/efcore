// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    // TODO: do this right after collection rewrite? Collection rewrite is what creates Where(x => true) calls
    public class NavigationExpansionCleanupVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.MethodIsClosedFormOf(LinqMethodHelpers.QueryableWhereMethodInfo)
                && methodCallExpression.Arguments[1].UnwrapQuote() is LambdaExpression lambda
                && lambda.Body is ConstantExpression constant
                && constant.Type == typeof(bool)
                && (bool)constant.Value)
            {
                var lambdaParameter = lambda.Parameters[0];
                var newLambda = Expression.Lambda(lambdaParameter, lambdaParameter);
                var newMethod = LinqMethodHelpers.QueryableSelectMethodInfo.MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType(), lambdaParameter.Type);

                return Expression.Call(newMethod, methodCallExpression.Arguments[0], newLambda);
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
