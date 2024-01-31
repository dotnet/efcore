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
        _sqlAliasManager = queryCompilationContext.SqlAliasManager;
        _useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryTranslationPostprocessorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override Expression Process(Expression query)
    {
        var query1 = base.Process(query);
        var query2 = new SelectExpressionProjectionApplyingExpressionVisitor(
            ((RelationalQueryCompilationContext)QueryCompilationContext).QuerySplittingBehavior).Visit(query1);
        var query3 = Prune(query2);

        // TODO: This - and all the verifications below - should happen after all visitors have run, including provider-specific ones.
        var query4 = _sqlAliasManager.PostprocessAliases(query3);

#if DEBUG
        // Verifies that all SelectExpression are marked as immutable after this point.
        new SelectExpressionMutableVerifyingExpressionVisitor().Visit(query4);
#endif

        var query5 = new SqlExpressionSimplifyingExpressionVisitor(RelationalDependencies.SqlExpressionFactory, _useRelationalNulls)
            .Visit(query4);
        var query6 = new RelationalValueConverterCompensatingExpressionVisitor(RelationalDependencies.SqlExpressionFactory).Visit(query5);

        return query6;
    }

    /// <summary>
    /// Prunes unnecessarily objects from the SQL tree, e.g. tables which aren't referenced by any column.
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
