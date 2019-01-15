// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public abstract class SqlExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case CaseExpression caseExpression:
                    return VisitCase(caseExpression);

                case ColumnExpression columnExpression:
                    return VisitColumn(columnExpression);

                case ExistsExpression existsExpression:
                    return VisitExists(existsExpression);

                case InExpression inExpression:
                    return VisitIn(inExpression);

                case LikeExpression likeExpression:
                    return VisitLike(likeExpression);

                case OrderingExpression orderingExpression:
                    return VisitOrdering(orderingExpression);

                case ProjectionExpression projectionExpression:
                    return VisitProjection(projectionExpression);

                case SelectExpression selectExpression:
                    return VisitSelect(selectExpression);

                case SqlBinaryExpression sqlBinaryExpression:
                    return VisitSqlBinary(sqlBinaryExpression);

                case SqlCastExpression sqlCastExpression:
                    return VisitSqlCast(sqlCastExpression);

                case SqlConstantExpression sqlConstantExpression:
                    return VisitSqlConstant(sqlConstantExpression);

                case SqlFragmentExpression sqlFragmentExpression:
                    return VisitSqlFragment(sqlFragmentExpression);

                case SqlFunctionExpression sqlFunctionExpression:
                    return VisitSqlFunction(sqlFunctionExpression);

                case SqlNegateExpression sqlNegateExpression:
                    return VisitSqlNegate(sqlNegateExpression);

                case SqlNotExpression sqlNotExpression:
                    return VisitSqlNot(sqlNotExpression);

                case SqlNullExpression sqlNullExpression:
                    return VisitSqlNull(sqlNullExpression);

                case SqlParameterExpression sqlParameterExpression:
                    return VisitSqlParameter(sqlParameterExpression);

                case TableExpression tableExpression:
                    return VisitTable(tableExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected abstract Expression VisitSqlNegate(SqlNegateExpression sqlNegateExpression);
        protected abstract Expression VisitExists(ExistsExpression existsExpression);
        protected abstract Expression VisitIn(InExpression inExpression);
        protected abstract Expression VisitProjection(ProjectionExpression projectionExpression);
        protected abstract Expression VisitCase(CaseExpression caseExpression);
        protected abstract Expression VisitSqlCast(SqlCastExpression sqlCastExpression);
        protected abstract Expression VisitSqlNot(SqlNotExpression sqlNotExpression);
        protected abstract Expression VisitSqlNull(SqlNullExpression sqlNullExpression);
        protected abstract Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression);
        protected abstract Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression);
        protected abstract Expression VisitOrdering(OrderingExpression orderingExpression);
        protected abstract Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression);
        protected abstract Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression);
        protected abstract Expression VisitColumn(ColumnExpression columnExpression);
        protected abstract Expression VisitSelect(SelectExpression selectExpression);
        protected abstract Expression VisitTable(TableExpression tableExpression);
        protected abstract Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression);
        protected abstract Expression VisitLike(LikeExpression likeExpression);
    }
}
