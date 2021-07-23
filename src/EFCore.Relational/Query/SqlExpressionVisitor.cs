// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A class that visits a SQL expression tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class SqlExpressionVisitor : ExpressionVisitor
    {
        /// <inheritdoc />
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

                case CollateExpression collateExpression:
                    return VisitCollate(collateExpression);

                case ColumnExpression columnExpression:
                    return VisitColumn(columnExpression);

                case CrossApplyExpression crossApplyExpression:
                    return VisitCrossApply(crossApplyExpression);

                case CrossJoinExpression crossJoinExpression:
                    return VisitCrossJoin(crossJoinExpression);

                case DistinctExpression distinctExpression:
                    return VisitDistinct(distinctExpression);

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

                case TableValuedFunctionExpression tableValuedFunctionExpression:
                    return VisitTableValuedFunction(tableValuedFunctionExpression);

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

        /// <summary>
        ///     Visits the children of the case expression.
        /// </summary>
        /// <param name="caseExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitCase(CaseExpression caseExpression);

        /// <summary>
        ///     Visits the children of the collate expression.
        /// </summary>
        /// <param name="collateExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitCollate(CollateExpression collateExpression);

        /// <summary>
        ///     Visits the children of the column expression.
        /// </summary>
        /// <param name="columnExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitColumn(ColumnExpression columnExpression);

        /// <summary>
        ///     Visits the children of the cross apply expression.
        /// </summary>
        /// <param name="crossApplyExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitCrossApply(CrossApplyExpression crossApplyExpression);

        /// <summary>
        ///     Visits the children of the cross join expression.
        /// </summary>
        /// <param name="crossJoinExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression);

        /// <summary>
        ///     Visits the children of the distinct expression.
        /// </summary>
        /// <param name="distinctExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitDistinct(DistinctExpression distinctExpression);

        /// <summary>
        ///     Visits the children of the except expression.
        /// </summary>
        /// <param name="exceptExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitExcept(ExceptExpression exceptExpression);

        /// <summary>
        ///     Visits the children of the exists expression.
        /// </summary>
        /// <param name="existsExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitExists(ExistsExpression existsExpression);

        /// <summary>
        ///     Visits the children of the from sql expression.
        /// </summary>
        /// <param name="fromSqlExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitFromSql(FromSqlExpression fromSqlExpression);

        /// <summary>
        ///     Visits the children of the in expression.
        /// </summary>
        /// <param name="inExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitIn(InExpression inExpression);

        /// <summary>
        ///     Visits the children of the intersect expression.
        /// </summary>
        /// <param name="intersectExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitIntersect(IntersectExpression intersectExpression);

        /// <summary>
        ///     Visits the children of the like expression.
        /// </summary>
        /// <param name="likeExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitLike(LikeExpression likeExpression);

        /// <summary>
        ///     Visits the children of the inner join expression.
        /// </summary>
        /// <param name="innerJoinExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression);

        /// <summary>
        ///     Visits the children of the left join expression.
        /// </summary>
        /// <param name="leftJoinExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression);

        /// <summary>
        ///     Visits the children of the ordering expression.
        /// </summary>
        /// <param name="orderingExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitOrdering(OrderingExpression orderingExpression);

        /// <summary>
        ///     Visits the children of the outer apply expression.
        /// </summary>
        /// <param name="outerApplyExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitOuterApply(OuterApplyExpression outerApplyExpression);

        /// <summary>
        ///     Visits the children of the projection expression.
        /// </summary>
        /// <param name="projectionExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitProjection(ProjectionExpression projectionExpression);

        /// <summary>
        ///     Visits the children of the table valued function expression.
        /// </summary>
        /// <param name="tableValuedFunctionExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitTableValuedFunction(TableValuedFunctionExpression tableValuedFunctionExpression);

        /// <summary>
        ///     Visits the children of the row number expression.
        /// </summary>
        /// <param name="rowNumberExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitRowNumber(RowNumberExpression rowNumberExpression);

        /// <summary>
        ///     Visits the children of the scalar subquery expression.
        /// </summary>
        /// <param name="scalarSubqueryExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression);

        /// <summary>
        ///     Visits the children of the select expression.
        /// </summary>
        /// <param name="selectExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitSelect(SelectExpression selectExpression);

        /// <summary>
        ///     Visits the children of the sql binary expression.
        /// </summary>
        /// <param name="sqlBinaryExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression);

        /// <summary>
        ///     Visits the children of the sql constant expression.
        /// </summary>
        /// <param name="sqlConstantExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression);

        /// <summary>
        ///     Visits the children of the sql fragent expression.
        /// </summary>
        /// <param name="sqlFragmentExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression);

        /// <summary>
        ///     Visits the children of the sql function expression.
        /// </summary>
        /// <param name="sqlFunctionExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression);

        /// <summary>
        ///     Visits the children of the sql parameter expression.
        /// </summary>
        /// <param name="sqlParameterExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression);

        /// <summary>
        ///     Visits the children of the sql unary expression.
        /// </summary>
        /// <param name="sqlUnaryExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression);

        /// <summary>
        ///     Visits the children of the table expression.
        /// </summary>
        /// <param name="tableExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitTable(TableExpression tableExpression);

        /// <summary>
        ///     Visits the children of the union expression.
        /// </summary>
        /// <param name="unionExpression"> The expression to visit. </param>
        /// <returns> The modified expression, if it or any subexpression was modified; otherwise, returns the original expression. </returns>
        protected abstract Expression VisitUnion(UnionExpression unionExpression);
    }
}
