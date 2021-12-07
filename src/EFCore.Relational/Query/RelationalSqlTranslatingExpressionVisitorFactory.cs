// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A factory for creating <see cref="RelationalSqlTranslatingExpressionVisitor" /> instances.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public class RelationalSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
{
    /// <summary>
    ///     Creates an instance of the <see cref="RelationalSqlTranslatingExpressionVisitorFactory" />.
    /// </summary>
    /// <param name="dependencies">The service dependencies.</param>
    public RelationalSqlTranslatingExpressionVisitorFactory(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual RelationalSqlTranslatingExpressionVisitorDependencies Dependencies { get; }

    /// <summary>
    ///     Creates a new <see cref="RelationalSqlTranslatingExpressionVisitor" />.
    /// </summary>
    /// <param name="queryCompilationContext">The query compilation context to use.</param>
    /// <param name="queryableMethodTranslatingExpressionVisitor">The visitor to use to translate sub-queries.</param>
    /// <returns>A relational sql translating expression visitor.</returns>
    public virtual RelationalSqlTranslatingExpressionVisitor Create(
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        => new(
            Dependencies,
            queryCompilationContext,
            queryableMethodTranslatingExpressionVisitor);
}
