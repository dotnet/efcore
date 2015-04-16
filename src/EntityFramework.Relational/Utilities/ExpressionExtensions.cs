// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
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

        public static ColumnExpression GetColumnExpression([NotNull] this Expression expression)
        {
            return (expression as AliasExpression)?.ColumnExpression();
        }

        public static bool IsAliasWithColumnExpression([NotNull] this Expression expression)
        {
            return (expression as AliasExpression)?.Expression is ColumnExpression;
        }

        public static bool HasColumnExpression([NotNull] this AliasExpression aliasExpression)
        {
            return aliasExpression?.Expression is ColumnExpression;
        }

        public static ColumnExpression ColumnExpression([NotNull] this AliasExpression aliasExpression)
        {
            return aliasExpression.Expression as ColumnExpression;
        }
    }
}
