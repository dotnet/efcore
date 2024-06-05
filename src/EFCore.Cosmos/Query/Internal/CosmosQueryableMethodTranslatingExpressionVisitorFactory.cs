// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryableMethodTranslatingExpressionVisitorFactory(
    QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
    ISqlExpressionFactory sqlExpressionFactory,
    ITypeMappingSource typeMappingSource,
    IMemberTranslatorProvider memberTranslatorProvider,
    IMethodCallTranslatorProvider methodCallTranslatorProvider)
    : IQueryableMethodTranslatingExpressionVisitorFactory
{
    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; } = dependencies;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new CosmosQueryableMethodTranslatingExpressionVisitor(
            Dependencies,
            (CosmosQueryCompilationContext)queryCompilationContext,
            sqlExpressionFactory,
            typeMappingSource,
            memberTranslatorProvider,
            methodCallTranslatorProvider);
}
