// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query
{
    public static class ExpressionEvaluationHelpers
    {
        public static object Evaluate(
            [CanBeNull] Expression expression,
            [CanBeNull] out string parameterName)
        {
            parameterName = null;

            if (expression == null)
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    var memberExpression = (MemberExpression)expression;
                    var @object = Evaluate(memberExpression.Expression, out parameterName);

                    var fieldInfo = memberExpression.Member as FieldInfo;

                    if (fieldInfo != null)
                    {
                        parameterName = parameterName != null
                            ? parameterName + "_" + fieldInfo.Name
                            : fieldInfo.Name;

                        try
                        {
                            return fieldInfo.GetValue(@object);
                        }
                        catch
                        {
                            // Try again when we compile the delegate
                        }
                    }

                    var propertyInfo = memberExpression.Member as PropertyInfo;

                    if (propertyInfo != null)
                    {
                        parameterName = parameterName != null
                            ? parameterName + "_" + propertyInfo.Name
                            : propertyInfo.Name;

                        try
                        {
                            return propertyInfo.GetValue(@object);
                        }
                        catch
                        {
                            // Try again when we compile the delegate
                        }
                    }

                    break;
                }
                case ExpressionType.Constant:
                {
                    return ((ConstantExpression)expression).Value;
                }
                case ExpressionType.Call:
                {
                    parameterName = ((MethodCallExpression)expression).Method.Name;

                    break;
                }
            }

            if (parameterName == null)
            {
                parameterName = "p";
            }

            return Expression.Lambda<Func<object>>(
                Expression.Convert(expression, typeof(object)))
                .Compile()
                .Invoke();
        }
    }
}
