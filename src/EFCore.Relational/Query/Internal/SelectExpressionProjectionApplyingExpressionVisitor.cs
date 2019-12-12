// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class SelectExpressionProjectionApplyingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            Check.NotNull(node, nameof(node));

            if (node is SelectExpression selectExpression)
            {
                selectExpression.ApplyProjection();
            }

            return base.VisitExtension(node);
        }
    }
}
