// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Expressions.Internal;

namespace Microsoft.Data.Entity.Query.Sql
{
    public interface ISqlServerExpressionVisitor
    {
        Expression VisitRowNumber([NotNull] RowNumberExpression columnExpression);
    }
}
