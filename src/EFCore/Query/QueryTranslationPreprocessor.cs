// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A class that preprocesses the query before translation.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class QueryTranslationPreprocessor
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryTranslationPreprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        public QueryTranslationPreprocessor(
            [NotNull] QueryTranslationPreprocessorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            Dependencies = dependencies;
            QueryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual QueryTranslationPreprocessorDependencies Dependencies { get; }

        /// <summary>
        ///     The query compilation context object for current compilation.
        /// </summary>
        protected virtual QueryCompilationContext QueryCompilationContext { get; }

        /// <summary>
        ///     Applies preprocessing transformations to the query.
        /// </summary>
        /// <param name="query"> The query to process. </param>
        /// <returns> A query expression after transformations. </returns>
        public virtual Expression Process([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = new InvocationExpressionRemovingExpressionVisitor().Visit(query);
            query = NormalizeQueryableMethod(query);
            query = new NullCheckRemovingExpressionVisitor().Visit(query);
            query = new SubqueryMemberPushdownExpressionVisitor(QueryCompilationContext.Model).Visit(query);
            query = new NavigationExpandingExpressionVisitor(this, QueryCompilationContext, Dependencies.EvaluatableExpressionFilter)
                .Expand(query);
            query = new QueryOptimizingExpressionVisitor().Visit(query);
            query = new NullCheckRemovingExpressionVisitor().Visit(query);

            return query;
        }

        /// <summary>
        ///     <para>
        ///         Normalizes queryable methods in the query.
        ///     </para>
        ///     <para>
        ///         This method extracts query metadata information like tracking, ignore query filters.
        ///         It also converts potential enumerable methods on navigation to queryable methods.
        ///         It flattens patterns of GroupJoin-SelectMany patterns to appropriate Join/LeftJoin.
        ///     </para>
        /// </summary>
        /// <param name="expression"> The query expression to normalize. </param>
        /// <returns> A query expression after normalization has been done. </returns>
        public virtual Expression NormalizeQueryableMethod([NotNull] Expression expression)
            => new QueryableMethodNormalizingExpressionVisitor(QueryCompilationContext)
                .Visit(Check.NotNull(expression, nameof(expression)));
    }
}
