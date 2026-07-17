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
    public override Expression Process(Expression query)
    {
        // Relational-only: rewrites GroupBy aggregates over reference navigations into explicit
        // pre-GroupBy left joins (#27933). Must run before the GroupJoin-flattening normalization,
        // and must not run for non-relational providers (Cosmos has no joins).
        // The synthesized tree is only correct under SQL translation (a joined hop may be null
        // after DefaultIfEmpty; SQL null propagation handles it, client evaluation would not).
        query = new GroupByAggregateNavigationLiftingExpressionVisitor(QueryCompilationContext.Model).Visit(query);

        return base.Process(query);
    }

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

    /// <inheritdoc />
    protected override bool IsEfConstantSupported
        => true;
}
