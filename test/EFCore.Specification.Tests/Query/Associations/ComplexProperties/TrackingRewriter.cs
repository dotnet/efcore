// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

internal class TrackingRewriter(QueryTrackingBehavior queryTrackingBehavior) : ExpressionVisitor
{
    private static readonly MethodInfo AsNoTrackingMethodInfo
        = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo()
            .GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking))!;

    private static readonly MethodInfo AsNoTrackingWithIdentityResolutionMethodInfo
        = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo()
            .GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTrackingWithIdentityResolution))!;

    protected override Expression VisitExtension(Expression expression)
        => expression is EntityQueryRootExpression root
            ? queryTrackingBehavior switch
            {
                QueryTrackingBehavior.NoTracking
                    => Expression.Call(AsNoTrackingMethodInfo.MakeGenericMethod(root.ElementType), root),
                QueryTrackingBehavior.NoTrackingWithIdentityResolution
                    => Expression.Call(AsNoTrackingWithIdentityResolutionMethodInfo.MakeGenericMethod(root.ElementType), root),
                QueryTrackingBehavior.TrackAll
                    => base.VisitExtension(expression),

                _ => throw new UnreachableException()
            }
            : base.VisitExtension(expression);
}
