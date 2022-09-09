// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryExpressionReplacingExpressionVisitor : ExpressionVisitor
{
    private readonly Expression _oldQuery;
    private readonly Expression _newQuery;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryExpressionReplacingExpressionVisitor(Expression oldQuery, Expression newQuery)
    {
        _oldQuery = oldQuery;
        _newQuery = newQuery;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
        => expression is ProjectionBindingExpression projectionBindingExpression
            && ReferenceEquals(projectionBindingExpression.QueryExpression, _oldQuery)
                ? projectionBindingExpression.ProjectionMember != null
                    ? new ProjectionBindingExpression(
                        _newQuery, projectionBindingExpression.ProjectionMember!, projectionBindingExpression.Type)
                    : new ProjectionBindingExpression(
                        _newQuery, projectionBindingExpression.Index!.Value, projectionBindingExpression.Type)
                : base.Visit(expression);
}
