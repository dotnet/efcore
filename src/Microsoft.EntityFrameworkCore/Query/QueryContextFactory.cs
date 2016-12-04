// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A factory for <see cref="QueryContext" /> instances.
    /// </summary>
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected QueryContextFactory(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IConcurrencyDetector concurrencyDetector)
        {
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(concurrencyDetector, nameof(concurrencyDetector));

            StateManager = new LazyRef<IStateManager>(() => currentContext.Context.GetService<IStateManager>());
            ConcurrencyDetector = concurrencyDetector;
            ChangeDetector = new LazyRef<IChangeDetector>(() => currentContext.Context.GetService<IChangeDetector>());
        }

        /// <summary>
        ///     Creates a query buffer.
        /// </summary>
        /// <returns>
        ///     The new query buffer.
        /// </returns>
        protected virtual IQueryBuffer CreateQueryBuffer()
            => new QueryBuffer(StateManager, ChangeDetector);

        /// <summary>
        ///     Gets the change detector.
        /// </summary>
        /// <value>
        ///     The change detector.
        /// </value>
        protected virtual LazyRef<IChangeDetector> ChangeDetector { get; }

        /// <summary>
        ///     Gets the state manager.
        /// </summary>
        /// <value>
        ///     The state manager.
        /// </value>
        protected virtual LazyRef<IStateManager> StateManager { get; }

        /// <summary>
        ///     Gets the concurrency detector.
        /// </summary>
        /// <value>
        ///     The concurrency detector.
        /// </value>
        protected virtual IConcurrencyDetector ConcurrencyDetector { get; }

        /// <summary>
        ///     Creates a new QueryContext.
        /// </summary>
        /// <returns>
        ///     A QueryContext.
        /// </returns>
        public abstract QueryContext Create();
    }
}
