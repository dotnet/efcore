// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SelectExpressionProjectionApplyingExpressionVisitor : ExpressionVisitor
{
    private readonly QuerySplittingBehavior _querySplittingBehavior;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SelectExpressionProjectionApplyingExpressionVisitor(QuerySplittingBehavior? querySplittingBehavior)
    {
        _querySplittingBehavior = querySplittingBehavior ?? QuerySplittingBehavior.SingleQuery;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            ShapedQueryExpression shapedQueryExpression
                when shapedQueryExpression.QueryExpression is SelectExpression selectExpression
                => shapedQueryExpression.UpdateShaperExpression(
                    selectExpression.ApplyProjection(
                        shapedQueryExpression.ShaperExpression, shapedQueryExpression.ResultCardinality, _querySplittingBehavior)),
            NonQueryExpression nonQueryExpression => nonQueryExpression,
            _ => base.VisitExtension(extensionExpression),
        };
}
