// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteEvaluatableExpressionFilter(
    EvaluatableExpressionFilterDependencies dependencies,
    RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
    : RelationalEvaluatableExpressionFilter(dependencies, relationalDependencies)
{
    /// <inheritdoc />
    public override bool IsEvaluatableExpression(Expression expression, IModel model)
        => (expression is not MethodCallExpression methodCallExpression
                || methodCallExpression.Method.DeclaringType != typeof(SqliteDbFunctionsExtensions)
                || methodCallExpression.Method.Name != nameof(SqliteDbFunctionsExtensions.JsonExtract))
            && base.IsEvaluatableExpression(expression, model);
}
