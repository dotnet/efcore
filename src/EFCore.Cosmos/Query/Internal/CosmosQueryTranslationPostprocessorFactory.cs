// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosQueryTranslationPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        Dependencies = dependencies;
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryTranslationPostprocessorDependencies Dependencies { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
        => new CosmosQueryTranslationPostprocessor(
            Dependencies,
            _sqlExpressionFactory,
            queryCompilationContext);
}
