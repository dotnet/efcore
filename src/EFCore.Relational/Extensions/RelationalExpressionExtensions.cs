// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class RelationalExpressionExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsSimpleExpression([NotNull] this Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var unwrappedExpression = expression.RemoveConvert();

            return unwrappedExpression is ConstantExpression
                   || unwrappedExpression is ColumnExpression
                   || unwrappedExpression is ParameterExpression
                   || unwrappedExpression is ColumnReferenceExpression
                   || unwrappedExpression is AliasExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Issue#11266 This method is being used by provider code. Do not break.
        public static ColumnReferenceExpression LiftExpressionFromSubquery(
            [NotNull] this Expression expression,
            [NotNull] TableExpressionBase table)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(table, nameof(table));

            switch (expression)
            {
                case ColumnExpression columnExpression:
                    return new ColumnReferenceExpression(columnExpression, table);
                case AliasExpression aliasExpression:
                    return new ColumnReferenceExpression(aliasExpression, table);
                case ColumnReferenceExpression columnReferenceExpression:
                    return new ColumnReferenceExpression(columnReferenceExpression, table);
            }

            Debug.Fail("LiftExpressionFromSubquery was called on incorrect expression type.");

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression UnwrapNullableExpression(this Expression expression)
            => expression is NullableExpression nullableExpression
                ? nullableExpression.Operand
                : expression;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Issue#11266 This method is being used by provider code. Do not break.
        public static IProperty FindProperty([NotNull] this Expression expression, [NotNull] Type targetType)
        {
            targetType = targetType.UnwrapNullableType();

            switch (expression)
            {
                case ColumnExpression columnExpression:
                    return columnExpression.Property;
                case ColumnReferenceExpression columnReferenceExpression:
                    return columnReferenceExpression.Expression.FindProperty(targetType);
                case AliasExpression aliasExpression:
                    return aliasExpression.Expression.FindProperty(targetType);
                case NullableExpression nullableExpression:
                    return nullableExpression.Operand.FindProperty(targetType);
                case UnaryExpression unaryExpression:
                    return unaryExpression.Operand.FindProperty(targetType);
                case SqlFunctionExpression functionExpression:
                    IEnumerable<Expression> arguments = functionExpression.Arguments;
                    if (functionExpression.Instance != null)
                    {
                        arguments = arguments.Concat(
                            new[] { functionExpression.Instance });
                    }

                    var properties = arguments
                        .Select(e => e.FindProperty(targetType))
                        .Where(p => p != null && p.ClrType.UnwrapNullableType() == targetType)
                        .ToList();

                    var property = properties.FirstOrDefault();
                    if (properties.Count > 1)
                    {
                        var mapping = property.FindRelationalMapping();
                        foreach (var otherProperty in properties)
                        {
                            if (otherProperty.FindRelationalMapping() != mapping)
                            {
                                // Issue #10006
                                return null;
                            }
                        }
                    }

                    return property;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ColumnExpression FindOriginatingColumnExpression([NotNull] this Expression expression)
        {
            switch (expression)
            {
                case ColumnExpression columnExpression:
                    return columnExpression;

                case ColumnReferenceExpression columnReferenceExpression:
                    return columnReferenceExpression.Expression.FindOriginatingColumnExpression();

                case AliasExpression aliasExpression:
                    return aliasExpression.Expression.FindOriginatingColumnExpression();

                case UnaryExpression unaryExpression:
                    return unaryExpression.Operand.FindOriginatingColumnExpression();
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression UnwrapAliasExpression(this Expression expression)
            => (expression as AliasExpression)?.Expression ?? expression;
    }
}
