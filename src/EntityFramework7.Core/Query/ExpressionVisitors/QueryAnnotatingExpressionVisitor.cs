// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query.Annotations;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class QueryAnnotatingExpressionVisitor : ExpressionVisitorBase
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.GetCustomAttribute<QueryAnnotationMethodAttribute>() != null)
            {
                string argumentName;

                methodCallExpression
                    = Expression.Call(
                        QueryAnnotationExtensions.QueryAnnotationMethodInfo
                            .MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()),
                        methodCallExpression.Arguments[0],
                        Expression.Constant(
                            new QueryAnnotation(
                                methodCallExpression.Method,
                                methodCallExpression.Arguments
                                    .Select(a => ExpressionEvaluationHelpers.Evaluate(a, out argumentName))
                                    .ToArray())));
            }
            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
