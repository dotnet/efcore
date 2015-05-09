// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace System.Linq.Expressions
{
    public static class ExpressionExtensions
    {
        public static bool IsLogicalOperation([NotNull] this Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression.NodeType == ExpressionType.AndAlso
                   || expression.NodeType == ExpressionType.OrElse;
        }

        public static ColumnExpression TryGetColumnExpression([NotNull] this Expression expression) 
            => (expression as AliasExpression)?.TryGetColumnExpression();

        public static bool IsAliasWithColumnExpression([NotNull] this Expression expression) 
            => (expression as AliasExpression)?.Expression is ColumnExpression;

        public static bool HasColumnExpression([CanBeNull] this AliasExpression aliasExpression) 
            => aliasExpression?.Expression is ColumnExpression;

        public static ColumnExpression TryGetColumnExpression([NotNull] this AliasExpression aliasExpression) 
            => aliasExpression.Expression as ColumnExpression;

        public static bool IsSimpleExpression([NotNull] this Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;

            if (unaryExpression != null
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                return IsSimpleExpression(unaryExpression.Operand);
            }

            return expression is ConstantExpression
                   || expression is ColumnExpression
                   || expression is ParameterExpression
                   || expression is LiteralExpression
                   || expression.IsAliasWithColumnExpression();
        }
    }
}
