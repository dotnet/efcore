// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
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

                case CrossApplyExpression crossApplyExpression:
                    return VisitCrossApply(crossApplyExpression);

                case CrossJoinExpression crossJoinExpression:
                    return VisitCrossJoin(crossJoinExpression);

                case ExceptExpression exceptExpression:
                    return VisitExcept(exceptExpression);

                case ExistsExpression existsExpression:
                    return VisitExists(existsExpression);

                case FromSqlExpression fromSqlExpression:
                    return VisitFromSql(fromSqlExpression);

                case InExpression inExpression:
                    return VisitIn(inExpression);

                case IntersectExpression intersectExpression:
                    return VisitIntersect(intersectExpression);

                case InnerJoinExpression innerJoinExpression:
                    return VisitInnerJoin(innerJoinExpression);

                case LeftJoinExpression leftJoinExpression:
                    return VisitLeftJoin(leftJoinExpression);

                case LikeExpression likeExpression:
                    return VisitLike(likeExpression);

                case OrderingExpression orderingExpression:
                    return VisitOrdering(orderingExpression);

                case OuterApplyExpression outerApplyExpression:
                    return VisitOuterApply(outerApplyExpression);

                case ProjectionExpression projectionExpression:
                    return VisitProjection(projectionExpression);

                case RowNumberExpression rowNumberExpression:
                    return VisitRowNumber(rowNumberExpression);

                case SelectExpression selectExpression:
                    return VisitSelect(selectExpression);

                case SqlBinaryExpression sqlBinaryExpression:
                    return VisitSqlBinary(sqlBinaryExpression);

                case SqlUnaryExpression sqlUnaryExpression:
                    return VisitSqlUnary(sqlUnaryExpression);

                case SqlConstantExpression sqlConstantExpression:
                    return VisitSqlConstant(sqlConstantExpression);

                case SqlFragmentExpression sqlFragmentExpression:
                    return VisitSqlFragment(sqlFragmentExpression);

                case SqlFunctionExpression sqlFunctionExpression:
                    return VisitSqlFunction(sqlFunctionExpression);

                case SqlParameterExpression sqlParameterExpression:
                    return VisitSqlParameter(sqlParameterExpression);

                case ScalarSubqueryExpression subSelectExpression:
                    return VisitSubSelect(subSelectExpression);

                case TableExpression tableExpression:
                    return VisitTable(tableExpression);

                case UnionExpression unionExpression:
                    return VisitUnion(unionExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected abstract Expression VisitCase(CaseExpression caseExpression);
        protected abstract Expression VisitColumn(ColumnExpression columnExpression);
        protected abstract Expression VisitCrossApply(CrossApplyExpression crossApplyExpression);
        protected abstract Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression);
        protected abstract Expression VisitExcept(ExceptExpression exceptExpression);
        protected abstract Expression VisitExists(ExistsExpression existsExpression);
        protected abstract Expression VisitFromSql(FromSqlExpression fromSqlExpression);
        protected abstract Expression VisitIn(InExpression inExpression);
        protected abstract Expression VisitIntersect(IntersectExpression intersectExpression);
        protected abstract Expression VisitLike(LikeExpression likeExpression);
        protected abstract Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression);
        protected abstract Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression);
        protected abstract Expression VisitOrdering(OrderingExpression orderingExpression);
        protected abstract Expression VisitOuterApply(OuterApplyExpression outerApplyExpression);
        protected abstract Expression VisitProjection(ProjectionExpression projectionExpression);
        protected abstract Expression VisitRowNumber(RowNumberExpression rowNumberExpression);
        protected abstract Expression VisitSelect(SelectExpression selectExpression);
        protected abstract Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression);
        protected abstract Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression);
        protected abstract Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression);
        protected abstract Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression);
        protected abstract Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression);
        protected abstract Expression VisitSqlUnary(SqlUnaryExpression sqlCastExpression);
        protected abstract Expression VisitSubSelect(ScalarSubqueryExpression scalarSubqueryExpression);
        protected abstract Expression VisitTable(TableExpression tableExpression);
        protected abstract Expression VisitUnion(UnionExpression unionExpression);
    }
}
