// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A factory for <see cref="QueryContext"/> instances.
    /// </summary>
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.QueryContextFactory class.
        /// </summary>
        /// <param name="stateManager"> The state manager. </param>
        /// <param name="concurrencyDetector"> The concurrency detector. </param>
        /// <param name="changeDetector"> The change detector. </param>
        protected QueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IChangeDetector changeDetector)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(concurrencyDetector, nameof(concurrencyDetector));
            Check.NotNull(changeDetector, nameof(changeDetector));

            StateManager = stateManager;
            ConcurrencyDetector = concurrencyDetector;
            ChangeDetector = changeDetector;
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
        protected virtual IChangeDetector ChangeDetector { get; }

        /// <summary>
        ///     Gets the state manager.
        /// </summary>
        /// <value>
        ///     The state manager.
        /// </value>
        protected virtual IStateManager StateManager { get; }

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
