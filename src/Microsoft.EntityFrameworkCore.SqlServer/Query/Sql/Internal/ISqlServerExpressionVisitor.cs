// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    public interface ISqlServerExpressionVisitor
    {
        Expression VisitRowNumber([NotNull] RowNumberExpression rowNumberExpression);
    }
}
