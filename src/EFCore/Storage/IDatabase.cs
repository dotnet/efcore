// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The main interaction point between a context and the database provider.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        ///     Persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <returns> The number of state entries persisted to the database. </returns>
        int SaveChanges([NotNull] IList<IUpdateEntry> entries);

        /// <summary>
        ///     Asynchronously persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of entries persisted to the database.
        /// </returns>
        Task<int> SaveChangesAsync(
            [NotNull] IList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Compiles the given query to generate a <see cref="Func{QueryContext, TResult}" />.
        /// </summary>
        /// <typeparam name="TResult"> The type of query result. </typeparam>
        /// <param name="query"> The query to compile. </param>
        /// <param name="async"> A value indicating whether this is an async query. </param>
        /// <returns> A <see cref="Func{QueryContext, TResult}" /> which can be invoked to get results of the query. </returns>
        Func<QueryContext, TResult> CompileQuery<TResult>([NotNull] Expression query, bool async);
    }
}
