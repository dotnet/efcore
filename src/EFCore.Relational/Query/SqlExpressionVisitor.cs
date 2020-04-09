// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SqlExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    return shapedQueryExpression.Update(
                        Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);

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

                case QueryableFunctionExpression queryableFunctionExpression:
                    return VisitQueryableFunctionExpression(queryableFunctionExpression);

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

                case ScalarSubqueryExpression scalarSubqueryExpression:
                    return VisitScalarSubquery(scalarSubqueryExpression);

                case TableExpression tableExpression:
                    return VisitTable(tableExpression);

                case UnionExpression unionExpression:
                    return VisitUnion(unionExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected abstract Expression VisitCase([NotNull] CaseExpression caseExpression);
        protected abstract Expression VisitColumn([NotNull] ColumnExpression columnExpression);
        protected abstract Expression VisitCrossApply([NotNull] CrossApplyExpression crossApplyExpression);
        protected abstract Expression VisitCrossJoin([NotNull] CrossJoinExpression crossJoinExpression);
        protected abstract Expression VisitExcept([NotNull] ExceptExpression exceptExpression);
        protected abstract Expression VisitExists([NotNull] ExistsExpression existsExpression);
        protected abstract Expression VisitFromSql([NotNull] FromSqlExpression fromSqlExpression);
        protected abstract Expression VisitIn([NotNull] InExpression inExpression);
        protected abstract Expression VisitIntersect([NotNull] IntersectExpression intersectExpression);
        protected abstract Expression VisitLike([NotNull] LikeExpression likeExpression);
        protected abstract Expression VisitInnerJoin([NotNull] InnerJoinExpression innerJoinExpression);
        protected abstract Expression VisitLeftJoin([NotNull] LeftJoinExpression leftJoinExpression);
        protected abstract Expression VisitOrdering([NotNull] OrderingExpression orderingExpression);
        protected abstract Expression VisitOuterApply([NotNull] OuterApplyExpression outerApplyExpression);
        protected abstract Expression VisitProjection([NotNull] ProjectionExpression projectionExpression);
        protected abstract Expression VisitQueryableFunctionExpression([NotNull] QueryableFunctionExpression queryableFunctionExpression);
        protected abstract Expression VisitRowNumber([NotNull] RowNumberExpression rowNumberExpression);
        protected abstract Expression VisitScalarSubquery([NotNull] ScalarSubqueryExpression scalarSubqueryExpression);
        protected abstract Expression VisitSelect([NotNull] SelectExpression selectExpression);
        protected abstract Expression VisitSqlBinary([NotNull] SqlBinaryExpression sqlBinaryExpression);
        protected abstract Expression VisitSqlConstant([NotNull] SqlConstantExpression sqlConstantExpression);
        protected abstract Expression VisitSqlFragment([NotNull] SqlFragmentExpression sqlFragmentExpression);
        protected abstract Expression VisitSqlFunction([NotNull] SqlFunctionExpression sqlFunctionExpression);
        protected abstract Expression VisitSqlParameter([NotNull] SqlParameterExpression sqlParameterExpression);
        protected abstract Expression VisitSqlUnary([NotNull] SqlUnaryExpression sqlUnaryExpression);
        protected abstract Expression VisitTable([NotNull] TableExpression tableExpression);
        protected abstract Expression VisitUnion([NotNull] UnionExpression unionExpression);
    }
}
