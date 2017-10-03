// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The main interaction point between a context and the database provider.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class Database : IDatabase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Database" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected Database([NotNull] DatabaseDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual DatabaseDependencies Dependencies { get; }

        /// <summary>
        ///     Persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <returns> The number of state entries persisted to the database. </returns>
        public abstract int SaveChanges(IReadOnlyList<IUpdateEntry> entries);

        /// <summary>
        ///     Asynchronously persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of entries persisted to the database.
        /// </returns>
        public abstract Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Translates a query model into a function that can be executed to get query results from the database.
        /// </summary>
        /// <typeparam name="TResult"> The type of results returned by the query. </typeparam>
        /// <param name="queryModel"> An object model representing the query to be executed. </param>
        /// <returns> A function that will execute the query. </returns>
        public virtual Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>(QueryModel queryModel)
            => Dependencies.QueryCompilationContextFactory
                .Create(async: false)
                .CreateQueryModelVisitor()
                .CreateQueryExecutor<TResult>(Check.NotNull(queryModel, nameof(queryModel)));

        /// <summary>
        ///     Translates a query model into a function that can be executed to asynchronously get query results from the database.
        /// </summary>
        /// <typeparam name="TResult"> The type of results returned by the query. </typeparam>
        /// <param name="queryModel"> An object model representing the query to be executed. </param>
        /// <returns> A function that will asynchronously execute the query. </returns>
        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
            => Dependencies.QueryCompilationContextFactory
                .Create(async: true)
                .CreateQueryModelVisitor()
                .CreateAsyncQueryExecutor<TResult>(Check.NotNull(queryModel, nameof(queryModel)));
    }
}
