// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosValueConverterCompensatingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosValueConverterCompensatingExpressionVisitor(
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            return extensionExpression switch
            {
                ShapedQueryExpression shapedQueryExpression => VisitShapedQueryExpression(shapedQueryExpression),
                ReadItemExpression readItemExpression => readItemExpression,
                SelectExpression selectExpression => VisitSelect(selectExpression),
                SqlConditionalExpression sqlConditionalExpression => VisitSqlConditional(sqlConditionalExpression),
                _ => base.VisitExtension(extensionExpression),
            };
        }

        private Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            return shapedQueryExpression.Update(
                Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);
        }

        private Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            var changed = false;

            var projections = new List<ProjectionExpression>();
            foreach (var item in selectExpression.Projection)
            {
                var updatedProjection = (ProjectionExpression)Visit(item);
                projections.Add(updatedProjection);
                changed |= updatedProjection != item;
            }

            var fromExpression = (RootReferenceExpression)Visit(selectExpression.FromExpression);
            changed |= fromExpression != selectExpression.FromExpression;

            var predicate = TryCompensateForBoolWithValueConverter((SqlExpression)Visit(selectExpression.Predicate));
            changed |= predicate != selectExpression.Predicate;

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in selectExpression.Orderings)
            {
                var orderingExpression = (SqlExpression)Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var limit = (SqlExpression)Visit(selectExpression.Limit);
            var offset = (SqlExpression)Visit(selectExpression.Offset);

            return changed
                ? selectExpression.Update(projections, fromExpression, predicate, orderings, limit, offset)
                : selectExpression;
        }

        private Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression)
        {
            Check.NotNull(sqlConditionalExpression, nameof(sqlConditionalExpression));

            var test = TryCompensateForBoolWithValueConverter((SqlExpression)Visit(sqlConditionalExpression.Test));
            var ifTrue = (SqlExpression)Visit(sqlConditionalExpression.IfTrue);
            var ifFalse = (SqlExpression)Visit(sqlConditionalExpression.IfFalse);

            return sqlConditionalExpression.Update(test, ifTrue, ifFalse);
        }

        private SqlExpression TryCompensateForBoolWithValueConverter(SqlExpression sqlExpression)
        {
            if (sqlExpression is KeyAccessExpression keyAccessExpression
                && keyAccessExpression.TypeMapping.ClrType == typeof(bool)
                && keyAccessExpression.TypeMapping.Converter != null)
            {
                return _sqlExpressionFactory.Equal(
                    sqlExpression,
                    _sqlExpressionFactory.Constant(true, sqlExpression.TypeMapping));
            }

            if (sqlExpression is SqlUnaryExpression sqlUnaryExpression)
            {
                return sqlUnaryExpression.Update(
                    TryCompensateForBoolWithValueConverter(sqlUnaryExpression.Operand));
            }

            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression
                && (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                    || sqlBinaryExpression.OperatorType == ExpressionType.OrElse))
            {
                return sqlBinaryExpression.Update(
                    TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Left),
                    TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Right));
            }

            return sqlExpression;
        }
    }
}
