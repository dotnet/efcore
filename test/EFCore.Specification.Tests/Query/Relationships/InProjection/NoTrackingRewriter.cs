// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class NoTrackingRewriter : ExpressionVisitor
{
    private static readonly MethodInfo AsNoTrackingMethodInfo
        = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking))!;

    protected override Expression VisitExtension(Expression expression)
    {
        if (expression is EntityQueryRootExpression eqr)
        {
            return Expression.Call(AsNoTrackingMethodInfo.MakeGenericMethod(eqr.ElementType), eqr);
        }

        return base.VisitExtension(expression);
    }
}
