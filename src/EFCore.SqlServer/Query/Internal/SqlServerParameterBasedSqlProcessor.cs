// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerParameterBasedSqlProcessor : RelationalParameterBasedSqlProcessor
{
    private readonly ISqlServerSingletonOptions _sqlServerSingletonOptions;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, parameters)
    {
        _sqlServerSingletonOptions = sqlServerSingletonOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Optimize(
        Expression queryExpression,
        Dictionary<string, object?> parametersValues,
        out bool canCache)
    {
        var optimizedQueryExpression = new SkipTakeCollapsingExpressionVisitor(Dependencies.SqlExpressionFactory)
            .Process(queryExpression, parametersValues, out var canCache2);

        optimizedQueryExpression = base.Optimize(optimizedQueryExpression, parametersValues, out canCache);

        canCache &= canCache2;

        return new SearchConditionConverter(Dependencies.SqlExpressionFactory).Visit(optimizedQueryExpression);
    }

    /// <inheritdoc />
    protected override Expression ProcessSqlNullability(
        Expression selectExpression,
        Dictionary<string, object?> parametersValues,
        out bool canCache)
    {
        Check.NotNull(selectExpression);
        Check.NotNull(parametersValues);

        return new SqlServerSqlNullabilityProcessor(Dependencies, Parameters, _sqlServerSingletonOptions).Process(
            selectExpression, parametersValues, out canCache);
    }
}
