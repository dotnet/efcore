// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalQueryTranslationPostprocessor : QueryTranslationPostprocessor
{
    private readonly SqlTreePruner _pruner = new();
    private readonly SqlAliasManager _sqlAliasManager;
    private readonly bool _useRelationalNulls;

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalQueryTranslationPostprocessor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        RelationalDependencies = relationalDependencies;
        RelationalQueryCompilationContext = queryCompilationContext;
        _sqlAliasManager = queryCompilationContext.SqlAliasManager;
        _useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryTranslationPostprocessorDependencies RelationalDependencies { get; }

    /// <summary>
    ///     The query compilation context object for current compilation.
    /// </summary>
    protected virtual RelationalQueryCompilationContext RelationalQueryCompilationContext { get; }

    /// <inheritdoc />
    public override Expression Process(Expression query)
    {
        var afterBase = base.Process(query);
        var afterTypeMappings = ProcessTypeMappings(afterBase);
        var afterProjectionApplication = new SelectExpressionProjectionApplyingExpressionVisitor(
            ((RelationalQueryCompilationContext)QueryCompilationContext).QuerySplittingBehavior)
            .Visit(afterTypeMappings);
        var afterPruning = Prune(afterProjectionApplication);

        // TODO: This - and all the verifications below - should happen after all visitors have run, including provider-specific ones.
        var afterAliases = _sqlAliasManager.PostprocessAliases(afterPruning);

#if DEBUG
        // Verifies that all SelectExpression are marked as immutable after this point.
        new SelectExpressionMutableVerifyingExpressionVisitor().Visit(afterAliases);
#endif

        var afterSimplification = new SqlExpressionSimplifyingExpressionVisitor(
                RelationalDependencies.SqlExpressionFactory, _useRelationalNulls)
            .Visit(afterAliases);
        var afterValueConverterCompensation =
            new RelationalValueConverterCompensatingExpressionVisitor(RelationalDependencies.SqlExpressionFactory)
                .Visit(afterSimplification);

        return afterValueConverterCompensation;
    }

    /// <summary>
    ///     Performs various postprocessing related to type mappings, e.g. applies inferred type mappings for queryable constants/parameters
    ///     and verifies that all <see cref="SqlExpression" /> have a type mapping.
    /// </summary>
    /// <param name="expression">The query expression to process.</param>
    protected virtual Expression ProcessTypeMappings(Expression expression)
        => new RelationalTypeMappingPostprocessor(Dependencies, RelationalDependencies, RelationalQueryCompilationContext).Process(expression);

    /// <summary>
    /// Prunes unnecessary objects from the SQL tree, e.g. tables which aren't referenced by any column.
    /// Can be overridden by providers for provider-specific pruning.
    /// </summary>
    protected virtual Expression Prune(Expression query)
        => _pruner.Prune(query);

#if DEBUG
    private sealed class SelectExpressionMutableVerifyingExpressionVisitor : ExpressionVisitor
    {
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case SelectExpression { IsMutable: true } selectExpression:
                    throw new InvalidDataException(selectExpression.Print());

                case ShapedQueryExpression shapedQueryExpression:
                    Visit(shapedQueryExpression.QueryExpression);
                    Visit(shapedQueryExpression.ShaperExpression);
                    return shapedQueryExpression;

                default:
                    return base.Visit(expression);
            }
        }
    }
#endif
}
