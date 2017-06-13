// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="QueryContext" />
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
    /// </summary>
    public sealed class QueryContextDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="QueryContext" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        /// </summary>
        public QueryContextDependencies(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IConcurrencyDetector concurrencyDetector)
        {
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(concurrencyDetector, nameof(concurrencyDetector));

            CurrentDbContext = currentContext;
            ConcurrencyDetector = concurrencyDetector;
        }

        /// <summary>
        ///     The cache being used to store value generator instances.
        /// </summary>
        public ICurrentDbContext CurrentDbContext { get; }

        /// <summary>
        ///     Gets the change detector.
        /// </summary>
        public IChangeDetector ChangeDetector => CurrentDbContext.GetDependencies().ChangeDetector;

        /// <summary>
        ///     Gets the state manager.
        /// </summary>
        public IStateManager StateManager => CurrentDbContext.GetDependencies().StateManager;

        /// <summary>
        ///     Gets the query provider.
        /// </summary>
        public IQueryProvider QueryProvider => CurrentDbContext.GetDependencies().QueryProvider;

        /// <summary>
        ///     Gets the concurrency detector.
        /// </summary>
        public IConcurrencyDetector ConcurrencyDetector { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="currentDbContext"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryContextDependencies With([NotNull] ICurrentDbContext currentDbContext)
            => new QueryContextDependencies(currentDbContext, ConcurrencyDetector);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="concurrencyDetector"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public QueryContextDependencies With([NotNull] IConcurrencyDetector concurrencyDetector)
            => new QueryContextDependencies(CurrentDbContext, concurrencyDetector);
    }
}
