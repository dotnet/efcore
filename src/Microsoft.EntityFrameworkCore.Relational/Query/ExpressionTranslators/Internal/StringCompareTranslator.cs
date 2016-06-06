// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class StringCompareTranslator : IExpressionFragmentTranslator
    {
        private static readonly Dictionary<ExpressionType, ExpressionType> _operatorMap = new Dictionary<ExpressionType, ExpressionType>
        {
            { ExpressionType.LessThan, ExpressionType.GreaterThan },
            { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThanOrEqual },
            { ExpressionType.GreaterThan, ExpressionType.LessThan },
            { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThanOrEqual },
            { ExpressionType.Equal, ExpressionType.Equal },
            { ExpressionType.NotEqual, ExpressionType.NotEqual }
        };

        private static readonly MethodInfo _methodInfo = typeof(string).GetTypeInfo()
            .GetDeclaredMethods(nameof(string.Compare))
            .Single(m => m.GetParameters().Length == 2);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
            {
                if (!_operatorMap.ContainsKey(expression.NodeType))
                {
                    return null;
                }

                var leftMethodCall = binaryExpression.Left as MethodCallExpression;
                var rightConstant = binaryExpression.Right as ConstantExpression;
                var translated = TranslateInternal(t => t, expression.NodeType, leftMethodCall, rightConstant);
                if (translated != null)
                {
                    return translated;
                }

                var leftConstant = binaryExpression.Left as ConstantExpression;
                var rightMethodCall = binaryExpression.Right as MethodCallExpression;
                var translatedReverse = TranslateInternal(t => _operatorMap[t], expression.NodeType, rightMethodCall, leftConstant);

                return translatedReverse;
            }

            return null;
        }

        private static Expression TranslateInternal(
            Func<ExpressionType, ExpressionType> opFunc,
            ExpressionType op,
            MethodCallExpression methodCall,
            ConstantExpression constant)
        {
            if ((methodCall != null)
                && (methodCall.Method == _methodInfo)
                && (methodCall.Type == typeof(int))
                && (constant != null)
                && (constant.Type == typeof(int)))
            {
                var arguments = methodCall.Arguments.ToList();
                var leftString = arguments[0];
                var rightString = arguments[1];
                var constantValue = (int)constant.Value;

                if (constantValue == 0)
                {
                    // Compare(strA, strB) > 0 => strA > strB
                    return new StringCompareExpression(opFunc(op), leftString, rightString);
                }

                if (constantValue == 1)
                {
                    if (op == ExpressionType.Equal)
                    {
                        // Compare(strA, strB) == 1 => strA > strB
                        return new StringCompareExpression(ExpressionType.GreaterThan, leftString, rightString);
                    }

                    if (op == opFunc(ExpressionType.LessThan))
                    {
                        // Compare(strA, strB) < 1 => strA <= strB
                        return new StringCompareExpression(ExpressionType.LessThanOrEqual, leftString, rightString);
                    }
                }

                if (constantValue == -1)
                {
                    if (op == ExpressionType.Equal)
                    {
                        // Compare(strA, strB) == -1 => strA < strB
                        return new StringCompareExpression(ExpressionType.LessThan, leftString, rightString);
                    }

                    if (op == opFunc(ExpressionType.GreaterThan))
                    {
                        // Compare(strA, strB) > -1 => strA >= strB
                        return new StringCompareExpression(ExpressionType.GreaterThanOrEqual, leftString, rightString);
                    }
                }
            }

            return null;
        }
    }
}
