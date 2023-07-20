// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class that preprocesses the query before translation.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class QueryTranslationPreprocessor
{
    /// <summary>
    ///     Creates a new instance of the <see cref="QueryTranslationPreprocessor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public QueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
    {
        Dependencies = dependencies;
        QueryCompilationContext = queryCompilationContext;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryTranslationPreprocessorDependencies Dependencies { get; }

    /// <summary>
    ///     The query compilation context object for current compilation.
    /// </summary>
    protected virtual QueryCompilationContext QueryCompilationContext { get; }

    /// <summary>
    ///     Applies preprocessing transformations to the query.
    /// </summary>
    /// <param name="query">The query to process.</param>
    /// <returns>A query expression after transformations.</returns>
    public virtual Expression Process(Expression query)
    {
        query = new InvocationExpressionRemovingExpressionVisitor().Visit(query);
        query = NormalizeQueryableMethod(query);
        query = new CallForwardingExpressionVisitor().Visit(query);
        query = new NullCheckRemovingExpressionVisitor().Visit(query);
        query = new SubqueryMemberPushdownExpressionVisitor(QueryCompilationContext.Model).Visit(query);
        query = new NavigationExpandingExpressionVisitor(
                this,
                QueryCompilationContext,
                Dependencies.EvaluatableExpressionFilter,
                Dependencies.NavigationExpansionExtensibilityHelper)
            .Expand(query);
        query = new QueryOptimizingExpressionVisitor().Visit(query);
        query = new NullCheckRemovingExpressionVisitor().Visit(query);

        return query;
    }

    /// <summary>
    ///     Normalizes queryable methods in the query.
    /// </summary>
    /// <remarks>
    ///     This method extracts query metadata information like tracking, ignore query filters.
    ///     It also converts potential enumerable methods on navigation to queryable methods.
    ///     It flattens patterns of GroupJoin-SelectMany patterns to appropriate Join/LeftJoin.
    /// </remarks>
    /// <param name="expression">The query expression to normalize.</param>
    /// <returns>A query expression after normalization has been done.</returns>
    public virtual Expression NormalizeQueryableMethod(Expression expression)
    {
        expression = new QueryableMethodNormalizingExpressionVisitor(QueryCompilationContext).Normalize(expression);
        expression = ProcessQueryRoots(expression);

        return expression;
    }

    /// <summary>
    ///     Adds additional query root nodes to the query.
    /// </summary>
    /// <param name="expression">The query expression to process.</param>
    /// <returns>A query expression after query roots have been added.</returns>
    protected virtual Expression ProcessQueryRoots(Expression expression)
        => new QueryRootProcessor(Dependencies, QueryCompilationContext).Visit(expression);
}
