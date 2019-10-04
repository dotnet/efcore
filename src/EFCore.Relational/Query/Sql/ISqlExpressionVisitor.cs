// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     Expression visitor dispatch methods for extension expressions.
    /// </summary>
    public interface ISqlExpressionVisitor
    {
        /// <summary>
        ///     Visit a ColumnExpression.
        /// </summary>
        /// <param name="columnExpression"> The column expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitColumn([NotNull] ColumnExpression columnExpression);

        /// <summary>
        ///     Visit an AliasExpression.
        /// </summary>
        /// <param name="aliasExpression"> The alias expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitAlias([NotNull] AliasExpression aliasExpression);

        /// <summary>
        ///     Visit an IsNullExpression.
        /// </summary>
        /// <param name="isNullExpression"> The is null expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitIsNull([NotNull] IsNullExpression isNullExpression);

        /// <summary>
        ///     Visit a LikeExpression.
        /// </summary>
        /// <param name="likeExpression"> The like expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitLike([NotNull] LikeExpression likeExpression);

        /// <summary>
        ///     Visit a SelectExpression.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitSelect([NotNull] SelectExpression selectExpression);

        /// <summary>
        ///     Visit a TableExpression.
        /// </summary>
        /// <param name="tableExpression"> The table expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitTable([NotNull] TableExpression tableExpression);

        /// <summary>
        ///     Visit a FromSqlExpression.
        /// </summary>
        /// <param name="fromSqlExpression"> from SQL expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitFromSql([NotNull] FromSqlExpression fromSqlExpression);

        /// <summary>
        ///     Visit a CrossJoinExpression.
        /// </summary>
        /// <param name="crossJoinExpression"> The cross join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitCrossJoin([NotNull] CrossJoinExpression crossJoinExpression);

        /// <summary>
        ///     Visit a CrossJoinLateralExpression.
        /// </summary>
        /// <param name="crossJoinLateralExpression"> The cross join lateral expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitCrossJoinLateral([NotNull] CrossJoinLateralExpression crossJoinLateralExpression);

        /// <summary>
        ///     Visit an InnerJoinExpression.
        /// </summary>
        /// <param name="innerJoinExpression"> The inner join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitInnerJoin([NotNull] InnerJoinExpression innerJoinExpression);

        /// <summary>
        ///     Visit a LeftOuterJoinExpression.
        /// </summary>
        /// <param name="leftOuterJoinExpression"> The left outer join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitLeftOuterJoin([NotNull] LeftOuterJoinExpression leftOuterJoinExpression);

        /// <summary>
        ///     Visits an ExistsExpression.
        /// </summary>
        /// <param name="existsExpression"> The exists expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitExists([NotNull] ExistsExpression existsExpression);

        /// <summary>
        ///     Visit an InExpression.
        /// </summary>
        /// <param name="inExpression"> The in expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitIn([NotNull] InExpression inExpression);

        /// <summary>
        ///     Visit a SqlFunctionExpression.
        /// </summary>
        /// <param name="sqlFunctionExpression"> The SQL function expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitSqlFunction([NotNull] SqlFunctionExpression sqlFunctionExpression);

        /// <summary>
        ///     Visit a StringCompareExpression.
        /// </summary>
        /// <param name="stringCompareExpression"> The string compare expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitStringCompare([NotNull] StringCompareExpression stringCompareExpression);

        /// <summary>
        ///     Visit an ExplicitCastExpression.
        /// </summary>
        /// <param name="explicitCastExpression"> The explicit cast expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitExplicitCast([NotNull] ExplicitCastExpression explicitCastExpression);

        /// <summary>
        ///     Visit a PropertyParameterExpression.
        /// </summary>
        /// <param name="propertyParameterExpression"> The property parameter expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitPropertyParameter([NotNull] PropertyParameterExpression propertyParameterExpression);

        /// <summary>
        ///     Visit a SqlFragmentExpression.
        /// </summary>
        /// <param name="sqlFragmentExpression"> The SqlFragmentExpression expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitSqlFragment([NotNull] SqlFragmentExpression sqlFragmentExpression);

        /// <summary>
        ///     Visit a ColumnReferenceExpression.
        /// </summary>
        /// <param name="columnReferenceExpression"> The ColumnReferenceExpression expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        Expression VisitColumnReference([NotNull] ColumnReferenceExpression columnReferenceExpression);

        /// <summary>
        /// Visits a case expression.
        /// </summary>
        /// <param name="caseExpression"> The case expression. </param>
        /// <returns> An expression. </returns>
        Expression VisitCase([NotNull] CaseExpression caseExpression);
    }
}
