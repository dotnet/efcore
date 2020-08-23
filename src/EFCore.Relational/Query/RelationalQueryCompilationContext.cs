// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         The primary data structure representing the state/components used during relational query compilation.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalQueryCompilationContext" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this class. </param>
        /// <param name="async"> A bool value indicating whether it is for async query. </param>
        public RelationalQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] RelationalQueryCompilationContextDependencies relationalDependencies,
            bool async)
            : base(dependencies, async)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
            QuerySplittingBehavior = RelationalOptionsExtension.Extract(ContextOptions).QuerySplittingBehavior;
        }

        /// <summary>
        ///     Parameter object containing relational service dependencies.
        /// </summary>
        protected virtual RelationalQueryCompilationContextDependencies RelationalDependencies { get; }

        /// <summary>
        ///     A value indicating the <see cref="EntityFrameworkCore.QuerySplittingBehavior" /> configured for the query.
        ///     If no value has been configured then <see cref="QuerySplittingBehavior.SingleQuery" /> will be used.
        /// </summary>
        public virtual QuerySplittingBehavior? QuerySplittingBehavior { get; internal set; }
    }
}
