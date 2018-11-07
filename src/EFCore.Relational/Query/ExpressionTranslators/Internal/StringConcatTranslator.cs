// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class StringConcatTranslator : IExpressionFragmentTranslator
    {
        private static readonly MethodInfo _stringConcatMethodInfo = typeof(string).GetTypeInfo()
            .GetDeclaredMethods(nameof(string.Concat))
            .Single(
                m => m.GetParameters().Length == 2
                     && m.GetParameters()[0].ParameterType == typeof(object)
                     && m.GetParameters()[1].ParameterType == typeof(object));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(Expression expression)
        {
            if (expression is BinaryExpression binaryExpression
                && binaryExpression.NodeType == ExpressionType.Add
                && _stringConcatMethodInfo.Equals(binaryExpression.Method))
            {
                var newLeft = binaryExpression.Left.Type != typeof(string)
                    ? new ExplicitCastExpression(HandleNullTypedConstant(binaryExpression.Left.RemoveConvert()), typeof(string))
                    : binaryExpression.Left;

                var newRight = binaryExpression.Right.Type != typeof(string)
                    ? new ExplicitCastExpression(HandleNullTypedConstant(binaryExpression.Right.RemoveConvert()), typeof(string))
                    : binaryExpression.Right;

                if (newLeft != binaryExpression.Left
                    || newRight != binaryExpression.Right)
                {
                    return Expression.Add(newLeft, newRight, _stringConcatMethodInfo);
                }
            }

            return null;
        }

        private static Expression HandleNullTypedConstant(Expression expression)
            => expression is ConstantExpression constantExpression
               && constantExpression.Type == typeof(object)
               && constantExpression.Value != null
                ? Expression.Constant(constantExpression.Value)
                : expression;
    }
}
