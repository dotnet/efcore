// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    public interface ISqlExpressionVisitor
    {
        Expression VisitColumn([NotNull] ColumnExpression columnExpression);
        Expression VisitAlias([NotNull] AliasExpression aliasExpression);
        Expression VisitIsNull([NotNull] IsNullExpression isNullExpression);
        Expression VisitLike([NotNull] LikeExpression likeExpression);
        Expression VisitSelect([NotNull] SelectExpression selectExpression);
        Expression VisitTable([NotNull] TableExpression tableExpression);
        Expression VisitFromSql([NotNull] FromSqlExpression fromSqlExpression);
        Expression VisitCrossJoin([NotNull] CrossJoinExpression crossJoinExpression);
        Expression VisitLateralJoin([NotNull] LateralJoinExpression lateralJoinExpression);
        Expression VisitInnerJoin([NotNull] InnerJoinExpression innerJoinExpression);
        Expression VisitLeftOuterJoin([NotNull] LeftOuterJoinExpression leftOuterJoinExpression);
        Expression VisitExists([NotNull] ExistsExpression existsExpression);
        Expression VisitCount([NotNull] CountExpression countExpression);
        Expression VisitSum([NotNull] SumExpression sumExpression);
        Expression VisitMin([NotNull] MinExpression minExpression);
        Expression VisitMax([NotNull] MaxExpression maxExpression);
        Expression VisitIn([NotNull] InExpression inExpression);
        Expression VisitSqlFunction([NotNull] SqlFunctionExpression sqlFunctionExpression);
        Expression VisitStringCompare([NotNull] StringCompareExpression stringCompareExpression);
        Expression VisitExplicitCast([NotNull] ExplicitCastExpression explicitCastExpression);
        Expression VisitPropertyParameter([NotNull] PropertyParameterExpression propertyParameterExpression);
    }
}
