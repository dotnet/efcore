// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
