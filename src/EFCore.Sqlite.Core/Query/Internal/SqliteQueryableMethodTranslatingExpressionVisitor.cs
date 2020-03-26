// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
    {
        public SqliteQueryableMethodTranslatingExpressionVisitor(
            [NotNull] QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
        }

        protected SqliteQueryableMethodTranslatingExpressionVisitor(
            [NotNull] SqliteQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor)
        {
        }

        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
            => new SqliteQueryableMethodTranslatingExpressionVisitor(this);

        protected override ShapedQueryExpression TranslateOrderBy(
            ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));

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
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));

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
            => (expression.TypeMapping?.Converter?.ProviderClrType
                ?? expression.TypeMapping?.ClrType
                ?? expression.Type).UnwrapNullableType();
    }
}
