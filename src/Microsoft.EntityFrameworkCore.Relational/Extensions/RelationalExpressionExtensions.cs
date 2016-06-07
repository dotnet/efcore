// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    public static class RelationalExpressionExtensions
    {
        public static ColumnExpression TryGetColumnExpression([NotNull] this Expression expression)
            => expression as ColumnExpression ?? (expression as AliasExpression)?.TryGetColumnExpression();

        public static bool IsAliasWithColumnExpression([NotNull] this Expression expression)
            => (expression as AliasExpression)?.Expression is ColumnExpression;

        public static bool IsAliasWithSelectExpression([NotNull] this Expression expression)
            => (expression as AliasExpression)?.Expression is SelectExpression;

        public static bool HasColumnExpression([CanBeNull] this AliasExpression aliasExpression)
            => aliasExpression?.Expression is ColumnExpression;

        public static ColumnExpression TryGetColumnExpression([NotNull] this AliasExpression aliasExpression)
            => aliasExpression.Expression as ColumnExpression;

        public static bool IsSimpleExpression([NotNull] this Expression expression)
        {
            var unwrappedExpression = expression.RemoveConvert();

            return unwrappedExpression is ConstantExpression
                   || unwrappedExpression is ColumnExpression
                   || unwrappedExpression is ParameterExpression
                   || unwrappedExpression.IsAliasWithColumnExpression();
        }
    }
}
