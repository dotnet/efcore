// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The principal data structure used by a compiled relational query during execution.
    /// </summary>
    public class RelationalQueryContext : QueryContext
    {
        private readonly List<IValueBufferCursor> _activeQueries = new List<IValueBufferCursor>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryContext(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IRelationalConnection connection,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory)
            : base(dependencies, queryBufferFactory)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(executionStrategyFactory, nameof(executionStrategyFactory));

            Connection = connection;
            ExecutionStrategyFactory = executionStrategyFactory;
        }

        /// <summary>
        ///     Gets the active relational connection.
        /// </summary>
        /// <value>
        ///     The connection.
        /// </value>
        public virtual IRelationalConnection Connection { get; }

        /// <summary>
        ///     Gets a semaphore used to serialize async queries.
        /// </summary>
        /// <value>
        ///     The semaphore.
        /// </value>
        public virtual SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

        /// <summary>
        ///     The execution strategy factory.
        /// </summary>
        /// <value>
        ///     The execution strategy factory.
        /// </value>
        public virtual IExecutionStrategyFactory ExecutionStrategyFactory { get; }

        /// <summary>
        ///     Registers a value buffer cursor.
        /// </summary>
        /// <param name="valueBufferCursor"> The value buffer cursor. </param>
        public virtual void RegisterValueBufferCursor([NotNull] IValueBufferCursor valueBufferCursor)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            if (_activeQueries.Count > 0
                && !Connection.IsMultipleActiveResultSetsEnabled)
            {
                _activeQueries.Last().BufferAll();
            }

            _activeQueries.Add(valueBufferCursor);
        }

        /// <summary>
        ///     Asynchronously registers a value buffer cursor.
        /// </summary>
        /// <param name="valueBufferCursor"> The value buffer cursor. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A Task.
        /// </returns>
        public virtual async Task RegisterValueBufferCursorAsync(
            [NotNull] IValueBufferCursor valueBufferCursor,
            CancellationToken cancellationToken)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            if (Connection.ActiveCursor != null
                && !Connection.IsMultipleActiveResultSetsEnabled)
            {
                await Connection.ActiveCursor.BufferAllAsync(cancellationToken);
            }

            Connection.ActiveCursor = valueBufferCursor;

            _activeQueries.Add(valueBufferCursor);
        }

        /// <summary>
        ///     Deregisters the value buffer cursor described by valueBufferCursor.
        /// </summary>
        /// <param name="valueBufferCursor"> The value buffer cursor. </param>
        public virtual void DeregisterValueBufferCursor([NotNull] IValueBufferCursor valueBufferCursor)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            Connection.ActiveCursor = null;

            _activeQueries.Remove(valueBufferCursor);
        }
    }
}
