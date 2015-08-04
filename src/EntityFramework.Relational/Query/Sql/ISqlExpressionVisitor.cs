// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.Sql
{
    public interface ISqlExpressionVisitor
    {
        Expression VisitColumn([NotNull] ColumnExpression columnExpression);
        Expression VisitAlias([NotNull] AliasExpression aliasExpression);
        Expression VisitIsNull([NotNull] IsNullExpression isNullExpression);
        Expression VisitLike([NotNull] LikeExpression likeExpression);
        Expression VisitLiteral([NotNull] LiteralExpression literalExpression);
        Expression VisitSelect([NotNull] SelectExpression selectExpression);
        Expression VisitTable([NotNull] TableExpression tableExpression);
        Expression VisitRawSqlDerivedTable([NotNull] RawSqlDerivedTableExpression rawSqlDerivedTableExpression);
        Expression VisitCrossJoin([NotNull] CrossJoinExpression crossJoinExpression);
        Expression VisitCrossApply([NotNull] CrossApplyExpression crossApplyExpression);
        Expression VisitInnerJoin([NotNull] InnerJoinExpression innerJoinExpression);
        Expression VisitOuterJoin([NotNull] LeftOuterJoinExpression leftOuterJoinExpression);
        Expression VisitExists([NotNull] ExistsExpression existsExpression);
        Expression VisitCount([NotNull] CountExpression countExpression);
        Expression VisitSum([NotNull] SumExpression sumExpression);
        Expression VisitMin([NotNull] MinExpression minExpression);
        Expression VisitMax([NotNull] MaxExpression maxExpression);
        Expression VisitIn([NotNull] InExpression inExpression);
        Expression VisitSqlFunction([NotNull] SqlFunctionExpression sqlFunctionExpression);
    }
}
