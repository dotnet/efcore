// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalQueryTranslationPreprocessor : QueryTranslationPreprocessor
{
    private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryTranslationPreprocessor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalQueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        RelationalDependencies = relationalDependencies;
        _relationalQueryCompilationContext = (RelationalQueryCompilationContext)queryCompilationContext;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryTranslationPreprocessorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override Expression NormalizeQueryableMethod(Expression expression)
    {
        expression = new RelationalQueryMetadataExtractingExpressionVisitor(_relationalQueryCompilationContext).Visit(expression);
        expression = base.NormalizeQueryableMethod(expression);

        return expression;
    }

    /// <inheritdoc />
    protected override Expression ProcessQueryRoots(Expression expression)
        => new RelationalQueryRootProcessor(Dependencies, RelationalDependencies, QueryCompilationContext).Visit(expression);
}
