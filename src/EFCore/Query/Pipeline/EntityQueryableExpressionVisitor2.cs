// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public abstract class EntityQueryableExpressionVisitor2 : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => constantExpression.IsEntityQueryable()
                ? CreateShapedQueryExpression(((IQueryable)constantExpression.Value).ElementType)
                : base.VisitConstant(constantExpression);

        protected abstract ShapedQueryExpression CreateShapedQueryExpression(Type elementType);
    }
}
