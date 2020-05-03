// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
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
    public class QueryTranslationPostprocessor
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryTranslationPostprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        public QueryTranslationPostprocessor(
            [NotNull] QueryTranslationPostprocessorDependencies dependencies,
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
        public virtual Expression Process([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            return query;
        }
    }
}
