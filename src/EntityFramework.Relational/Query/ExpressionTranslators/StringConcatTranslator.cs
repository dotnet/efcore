// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public class StringConcatTranslator : IExpressionFragmentTranslator
    {
        private static MethodInfo _stringConcatMethodInfo = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Concat))
            .Where(m => m.GetParameters().Count() == 2 && m.GetParameters()[0].ParameterType == typeof(object) && m.GetParameters()[1].ParameterType == typeof(object))
            .Single();


        public virtual Expression Translate([NotNull] Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null && binaryExpression.NodeType == ExpressionType.Add && binaryExpression.Method == _stringConcatMethodInfo)
            {
                var newLeft = binaryExpression.Left.Type != typeof(string)
                    ? new ExplicitCastExpression(HandleNullTypedConstant(binaryExpression.Left.RemoveConvert()), typeof(string))
                    : binaryExpression.Left;

                var newRight = binaryExpression.Right.Type != typeof(string)
                    ? new ExplicitCastExpression(HandleNullTypedConstant(binaryExpression.Right.RemoveConvert()), typeof(string))
                    : binaryExpression.Right;

                if (newLeft != binaryExpression.Left || newRight != binaryExpression.Right)
                {
                    return Expression.Add(newLeft, newRight, _stringConcatMethodInfo);
                }
            }

            return null;
        }

        private static Expression HandleNullTypedConstant(Expression expression)
        {
            var constantExpression = expression as ConstantExpression;

            var newExpression = constantExpression != null && constantExpression.Type == typeof(object) && constantExpression.Value != null
                ? Expression.Constant(constantExpression.Value)
                : expression;

            return newExpression;
        }
    }
}
