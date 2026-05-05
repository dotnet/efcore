// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    private readonly SqlServerJsonPostprocessor _jsonPostprocessor;
    private readonly SqlServerAggregateOverSubqueryPostprocessor _aggregatePostprocessor;
    private readonly SkipWithoutOrderByInSplitQueryVerifier _skipWithoutOrderByInSplitQueryVerifier = new();
    private readonly SqlServerSqlTreePruner _pruner = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        SqlServerQueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _jsonPostprocessor = new SqlServerJsonPostprocessor(
            relationalDependencies.TypeMappingSource, relationalDependencies.SqlExpressionFactory, queryCompilationContext.SqlAliasManager);
        _aggregatePostprocessor = new SqlServerAggregateOverSubqueryPostprocessor(queryCompilationContext.SqlAliasManager);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        var query1 = base.Process(query);

        var query2 = _jsonPostprocessor.Process(query1);
        var query3 = _aggregatePostprocessor.Visit(query2);
        _skipWithoutOrderByInSplitQueryVerifier.Visit(query3);

        return query3;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ProcessTypeMappings(Expression expression)
        => new SqlServerTypeMappingPostprocessor(Dependencies, RelationalDependencies, RelationalQueryCompilationContext).Process(
            expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression Prune(Expression query)
        => _pruner.Prune(query);

    private sealed class SkipWithoutOrderByInSplitQueryVerifier : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    Visit(shapedQueryExpression.ShaperExpression);
                    return shapedQueryExpression;

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    foreach (var table in relationalSplitCollectionShaperExpression.SelectExpression.Tables)
                    {
                        Visit(table);
                    }

                    Visit(relationalSplitCollectionShaperExpression.InnerShaper);

                    return relationalSplitCollectionShaperExpression;

                case SelectExpression { Offset: not null, Orderings.Count: 0 }:
                    throw new InvalidOperationException(SqlServerStrings.SplitQueryOffsetWithoutOrderBy);

                case NonQueryExpression nonQueryExpression:
                    return nonQueryExpression;

                default:
                    return base.Visit(expression);
            }
        }
    }
}
