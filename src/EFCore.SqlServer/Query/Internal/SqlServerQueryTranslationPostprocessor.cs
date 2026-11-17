// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryTranslationPostprocessor(
    QueryTranslationPostprocessorDependencies dependencies,
    RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
    SqlServerQueryCompilationContext queryCompilationContext)
    : RelationalQueryTranslationPostprocessor(dependencies, relationalDependencies, queryCompilationContext)
{
    private static readonly HashSet<string> VolatileFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "NEWID", "RAND", "CRYPT_GEN_RANDOM"
    };

    private readonly SqlServerJsonPostprocessor _jsonPostprocessor = new(
        relationalDependencies.TypeMappingSource,
        relationalDependencies.SqlExpressionFactory,
        queryCompilationContext.SqlAliasManager);

    private readonly SqlServerAggregateArgumentPostprocessor _aggregatePostprocessor = new(queryCompilationContext.SqlAliasManager);
    private readonly SqlServerSqlTreePruner _pruner = new();
    private readonly bool _supportsJsonTypeInOpenJson =
        queryCompilationContext.ContextOptions.FindExtension<SqlServerOptionsExtension>()! is
            { EngineType: SqlServerEngineType.SqlServer, SqlServerCompatibilityLevel: >= 170 };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        var query1 = base.Process(query);

        var query2 = _jsonPostprocessor.Process(query1, _supportsJsonTypeInOpenJson);
        var query3 = _aggregatePostprocessor.Visit(query2);

        new SqlServerVectorSearchWithoutApproximateDetector(queryCompilationContext).Visit(query3);

        return query3;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ProcessTypeMappings(Expression expression)
        => new SqlServerTypeMappingPostprocessor(Dependencies, RelationalDependencies, RelationalQueryCompilationContext).Process(
            expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression Prune(Expression query)
    {
        var merger = new SingleRowSubqueryMergingExpressionVisitor(queryCompilationContext.SqlAliasManager, VolatileFunctions);

        // Pruning narrows each copy's nested tables differently, so the top-level select, whose projection pruning keeps, merges first.
        // Other selects merge after pruning, so that a read pruning removes can't justify an apply.
        return merger.Visit(_pruner.Prune(merger.MergeTopLevelSelect(query)));
    }
}
