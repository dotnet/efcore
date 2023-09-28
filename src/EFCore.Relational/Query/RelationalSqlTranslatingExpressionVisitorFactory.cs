// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
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

    /// <inheritdoc />
    public virtual RelationalSqlTranslatingExpressionVisitor Create(
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        => new(
            Dependencies,
            queryCompilationContext,
            queryableMethodTranslatingExpressionVisitor);
}
