// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public interface ISqlExpressionVisitor
    {
        Expression VisitPropertyAccessExpression([NotNull] PropertyAccessExpression expression);
        Expression VisitIsNullExpression([NotNull] IsNullExpression expression);
        Expression VisitIsNotNullExpression([NotNull] IsNotNullExpression expression);
        Expression VisitLikeExpression([NotNull] LikeExpression expression);
        Expression VisitLiteralExpression([NotNull] LiteralExpression expression);
        Expression VisitSelectExpression([NotNull] SelectExpression expression);
    }
}
