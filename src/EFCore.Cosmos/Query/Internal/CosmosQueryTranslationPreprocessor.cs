// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryTranslationPreprocessor : QueryTranslationPreprocessor
{
    private readonly CosmosQueryCompilationContext _queryCompilationContext;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosQueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        CosmosQueryCompilationContext cosmosQueryCompilationContext)
        : base(dependencies, cosmosQueryCompilationContext)
    {
        _queryCompilationContext = cosmosQueryCompilationContext;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression NormalizeQueryableMethod(Expression query)
    {
        query = new CosmosQueryMetadataExtractingExpressionVisitor(_queryCompilationContext).Visit(query);
        query = base.NormalizeQueryableMethod(query);

        return query;
    }
}
