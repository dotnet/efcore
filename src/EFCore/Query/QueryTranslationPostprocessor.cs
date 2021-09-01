// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A class that postprocesses the translated query.
    ///         This class allows to process the generated server query expression and the associated shaper expression.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information.
    /// </remarks>
    public class QueryTranslationPostprocessor
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryTranslationPostprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        public QueryTranslationPostprocessor(
            QueryTranslationPostprocessorDependencies dependencies,
            QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            Dependencies = dependencies;
            QueryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual QueryTranslationPostprocessorDependencies Dependencies { get; }

        /// <summary>
        ///     The query compilation context object for current compilation.
        /// </summary>
        protected virtual QueryCompilationContext QueryCompilationContext { get; }

        /// <summary>
        ///     Applies postprocessing transformations to the translated query.
        /// </summary>
        /// <param name="query"> The query to process. </param>
        /// <returns> A query expression after transformations. </returns>
        public virtual Expression Process(Expression query)
        {
            Check.NotNull(query, nameof(query));

            return query;
        }
    }
}
