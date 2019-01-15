// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="Database" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed class DatabaseDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="Database" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="queryCompilationContextFactory"> Factory for compilation contexts to process LINQ queries. </param>
        /// <param name="queryCompilationContextFactory2"> A </param>
        public DatabaseDependencies([NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            IQueryCompilationContextFactory2 queryCompilationContextFactory2)
        {
            Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory));

            QueryCompilationContextFactory = queryCompilationContextFactory;
            QueryCompilationContextFactory2 = queryCompilationContextFactory2;
        }

        /// <summary>
        ///     Factory for compilation contexts to process LINQ queries.
        /// </summary>
        public IQueryCompilationContextFactory QueryCompilationContextFactory { get; }
        public IQueryCompilationContextFactory2 QueryCompilationContextFactory2 { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryCompilationContextFactory">
        ///     A replacement for the current dependency of this type.
        /// </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public DatabaseDependencies With([NotNull] IQueryCompilationContextFactory queryCompilationContextFactory)
            => new DatabaseDependencies(Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory)),
                QueryCompilationContextFactory2);

        public DatabaseDependencies With([NotNull] IQueryCompilationContextFactory2 queryCompilationContextFactory2)
            => new DatabaseDependencies(QueryCompilationContextFactory,
                Check.NotNull(queryCompilationContextFactory2, nameof(queryCompilationContextFactory2)));
    }
}
