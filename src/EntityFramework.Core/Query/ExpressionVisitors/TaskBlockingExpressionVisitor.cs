// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class TaskBlockingExpressionVisitor : ExpressionVisitorBase, ITaskBlockingExpressionVisitor
    {
        public override Expression Visit(Expression expression)
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

            return base.Visit(expression);
        }

        private static readonly MethodInfo _resultMethodInfo
            = typeof(TaskBlockingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Result));

        [UsedImplicitly]
        private static T _Result<T>(Task<T> task) => task.Result;
    }
}
