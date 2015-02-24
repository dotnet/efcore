// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class TaskBlockingExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        public override Expression VisitExpression([CanBeNull] Expression expression)
        {
            if (expression != null)
            {
                var typeInfo = expression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType
                    && typeInfo.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return Expression.Call(
                        _resultMethodInfo.MakeGenericMethod(typeInfo.GenericTypeArguments[0]),
                        expression);
                }
            }

            return base.VisitExpression(expression);
        }

        private static readonly MethodInfo _resultMethodInfo
            = typeof(TaskBlockingExpressionTreeVisitor).GetTypeInfo()
                .GetDeclaredMethod("_Result");

        [UsedImplicitly]
        private static T _Result<T>(Task<T> task)
        {
            return task.Result;
        }
    }
}
