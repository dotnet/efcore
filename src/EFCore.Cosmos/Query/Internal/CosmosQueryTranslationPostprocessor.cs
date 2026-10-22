// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryTranslationPostprocessor(
    QueryTranslationPostprocessorDependencies dependencies,
    ISqlExpressionFactory sqlExpressionFactory,
    CosmosQueryCompilationContext queryCompilationContext)
    : QueryTranslationPostprocessor(dependencies, queryCompilationContext)
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        query = base.Process(query);

        if (query is ShapedQueryExpression { QueryExpression: SelectExpression selectExpression })
        {
            selectExpression.ApplyProjection();
        }

        var afterValueConverterCompensation = new CosmosValueConverterCompensatingExpressionVisitor(sqlExpressionFactory).Visit(query);
        var afterAliases = queryCompilationContext.AliasManager.PostprocessAliases(afterValueConverterCompensation);
        var afterExtraction = new CosmosReadItemAndPartitionKeysExtractor().ExtractPartitionKeysAndId(
            queryCompilationContext, sqlExpressionFactory, afterAliases);

        return afterExtraction;
    }
}
