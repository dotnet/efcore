// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public interface ISqlExpressionVisitor
    {
        Expression VisitColumnExpression([NotNull] ColumnExpression columnExpression);
        Expression VisitIsNullExpression([NotNull] IsNullExpression isNullExpression);
        Expression VisitIsNotNullExpression([NotNull] IsNotNullExpression isNotNullExpression);
        Expression VisitLikeExpression([NotNull] LikeExpression likeExpression);
        Expression VisitLiteralExpression([NotNull] LiteralExpression literalExpression);
        Expression VisitSelectExpression([NotNull] SelectExpression selectExpression);
        Expression VisitTableExpression([NotNull] TableExpression tableExpression);
        Expression VisitCrossJoinExpression([NotNull] CrossJoinExpression crossJoinExpression);
        Expression VisitInnerJoinExpression([NotNull] InnerJoinExpression innerJoinExpression);
        Expression VisitCaseExpression([NotNull] CaseExpression caseExpression);
        Expression VisitExistsExpression([NotNull] ExistsExpression existsExpression);
        Expression VisitCountExpression([NotNull] CountExpression countExpression);
    }
}
