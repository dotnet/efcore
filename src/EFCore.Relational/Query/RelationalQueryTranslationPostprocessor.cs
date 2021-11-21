// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalQueryTranslationPostprocessor : QueryTranslationPostprocessor
{
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
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        RelationalDependencies = relationalDependencies;
        _useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryTranslationPostprocessorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override Expression Process(Expression query)
    {
        query = base.Process(query);
        query = new SelectExpressionProjectionApplyingExpressionVisitor(
            ((RelationalQueryCompilationContext)QueryCompilationContext).QuerySplittingBehavior).Visit(query);
#if DEBUG
        query = new TableAliasVerifyingExpressionVisitor().Visit(query);
#endif
        query = new SelectExpressionPruningExpressionVisitor().Visit(query);
        query = new SqlExpressionSimplifyingExpressionVisitor(RelationalDependencies.SqlExpressionFactory, _useRelationalNulls)
            .Visit(query);
        query = new RelationalValueConverterCompensatingExpressionVisitor(RelationalDependencies.SqlExpressionFactory).Visit(query);

        return query;
    }

    private sealed class TableAliasVerifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly ScopedVisitor _scopedVisitor = new();

        // Validates that all aliases are unique inside SelectExpression
        // And all aliases are used in without any generated alias being missing
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    UniquifyAliasInSelectExpression(shapedQueryExpression.QueryExpression);
                    Visit(shapedQueryExpression.QueryExpression);
                    return shapedQueryExpression;

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    UniquifyAliasInSelectExpression(relationalSplitCollectionShaperExpression.SelectExpression);
                    Visit(relationalSplitCollectionShaperExpression.InnerShaper);
                    return relationalSplitCollectionShaperExpression;

                default:
                    return base.Visit(expression);
            }
        }

        private void UniquifyAliasInSelectExpression(Expression selectExpression)
            => _scopedVisitor.EntryPoint(selectExpression);

        private sealed class ScopedVisitor : ExpressionVisitor
        {
            private readonly HashSet<string> _usedAliases = new(StringComparer.OrdinalIgnoreCase);
            private readonly HashSet<TableExpressionBase> _visitedTableExpressionBases = new(ReferenceEqualityComparer.Instance);

            public Expression EntryPoint(Expression expression)
            {
                _usedAliases.Clear();
                _visitedTableExpressionBases.Clear();

                var result = Visit(expression);

                foreach (var group in _usedAliases.GroupBy(e => e[..1]))
                {
                    if (group.Count() == 1)
                    {
                        continue;
                    }

                    var numbers = group.OrderBy(e => e).Skip(1).Select(e => int.Parse(e[1..])).OrderBy(e => e).ToList();
                    if (numbers.Count - 1 != numbers[^1])
                    {
                        throw new InvalidOperationException($"Missing alias in the list: {string.Join(",", group.Select(e => e))}");
                    }
                }

                return result;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                var visitedExpression = base.Visit(expression);
                if (visitedExpression is TableExpressionBase tableExpressionBase
                    && !_visitedTableExpressionBases.Contains(tableExpressionBase)
                    && tableExpressionBase.Alias != null)
                {
                    if (_usedAliases.Contains(tableExpressionBase.Alias))
                    {
                        throw new InvalidOperationException($"Duplicate alias: {tableExpressionBase.Alias}");
                    }

                    _usedAliases.Add(tableExpressionBase.Alias);

                    _visitedTableExpressionBases.Add(tableExpressionBase);
                }

                return visitedExpression;
            }
        }
    }
}
