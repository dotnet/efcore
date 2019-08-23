// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
    {
        public SqliteQueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
            IModel model)
            : base(dependencies, relationalDependencies, model)
        {
        }

        protected SqliteQueryableMethodTranslatingExpressionVisitor(
            SqliteQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor)
        {
        }

        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
            => new SqliteQueryableMethodTranslatingExpressionVisitor(this);

        protected override ShapedQueryExpression TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var translation = base.TranslateOrderBy(source, keySelector, ascending);
            if (translation == null)
            {
                return null;
            }

            var orderingExpression = ((SelectExpression)translation.QueryExpression).Orderings.Last();
            var orderingExpressionType = GetProviderType(orderingExpression.Expression);
            if (orderingExpressionType == typeof(DateTimeOffset)
                || orderingExpressionType == typeof(decimal)
                || orderingExpressionType == typeof(TimeSpan)
                || orderingExpressionType == typeof(ulong))
            {
                throw new NotSupportedException(
                    SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
            }

            return translation;
        }

        protected override ShapedQueryExpression TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var translation = base.TranslateThenBy(source, keySelector, ascending);
            if (translation == null)
            {
                return null;
            }

            var orderingExpression = ((SelectExpression)translation.QueryExpression).Orderings.Last();
            var orderingExpressionType = GetProviderType(orderingExpression.Expression);
            if (orderingExpressionType == typeof(DateTimeOffset)
                || orderingExpressionType == typeof(decimal)
                || orderingExpressionType == typeof(TimeSpan)
                || orderingExpressionType == typeof(ulong))
            {
                throw new NotSupportedException(
                    SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
            }

            return translation;
        }

        private static Type GetProviderType(SqlExpression expression)
            => SharedTypeExtensions.UnwrapNullableType(
                expression.TypeMapping?.Converter?.ProviderClrType
                    ?? expression.TypeMapping?.ClrType
                    ?? expression.Type);

    }
}
