// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public class TrackingRewriter : ExpressionVisitor
{
    private readonly bool _noTrackingWithIdentityResolution;

    public TrackingRewriter(bool noTrackingWithIdentityResolution = false)
    {
        _noTrackingWithIdentityResolution = noTrackingWithIdentityResolution;
    }

    private static readonly MethodInfo AsNoTrackingMethodInfo
        = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking))!;

    private static readonly MethodInfo AsNoTrackingWithIdentityResolutionMethodInfo
        = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTrackingWithIdentityResolution))!;

    protected override Expression VisitExtension(Expression expression)
    {
        if (expression is EntityQueryRootExpression eqr)
        {
            return _noTrackingWithIdentityResolution
                ? Expression.Call(AsNoTrackingWithIdentityResolutionMethodInfo.MakeGenericMethod(eqr.ElementType), eqr)
                : Expression.Call(AsNoTrackingMethodInfo.MakeGenericMethod(eqr.ElementType), eqr);
        }

        return base.VisitExtension(expression);
    }
}
