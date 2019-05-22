// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extension methods for <see cref="Expression" /> typically used by database providers to
    ///         help in LINQ translation.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static class RelationalExpressionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Finds the <see cref="IProperty" /> associated with the given <see cref="Expression" />.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="targetType"> The target <see cref="Type" /> for the property. </param>
        /// <returns> The found property, or <code>null</code> if none was found. </returns>
        public static IProperty FindProperty([NotNull] this Expression expression, [NotNull] Type targetType)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(targetType, nameof(targetType));

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
                            new[]
                            {
                                functionExpression.Instance
                            });
                    }

                    targetType = targetType.UnwrapNullableType();
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
    }
}
