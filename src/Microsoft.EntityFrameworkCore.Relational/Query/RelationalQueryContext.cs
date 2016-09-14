// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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
        private readonly List<IValueBufferCursor> _activeIncludeQueries = new List<IValueBufferCursor>();

        private int _activeIncludeQueryOffset;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryContext(
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IRelationalConnection connection,
            [NotNull] LazyRef<IStateManager> stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory)
            : base(
                Check.NotNull(queryBufferFactory, nameof(queryBufferFactory)),
                Check.NotNull(stateManager, nameof(stateManager)),
                Check.NotNull(concurrencyDetector, nameof(concurrencyDetector)))
        {
            Check.NotNull(connection, nameof(connection));

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
        /// <param name="queryIndex"> Zero-based index of the query. </param>
        public virtual void RegisterValueBufferCursor(
            [NotNull] IValueBufferCursor valueBufferCursor, int? queryIndex)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            if (_activeQueries.Count > 0
                && !Connection.IsMultipleActiveResultSetsEnabled)
            {
                _activeQueries.Last().BufferAll();
            }

            _activeQueries.Add(valueBufferCursor);

            if (queryIndex.HasValue
                && queryIndex.Value > 0)
            {
                AddBufferCursorToIncludeQueriesList(valueBufferCursor, queryIndex.Value);
            }
        }

        /// <summary>
        ///     Asynchronously registers a value buffer cursor.
        /// </summary>
        /// <param name="valueBufferCursor"> The value buffer cursor. </param>
        /// <param name="queryIndex"> Zero-based index of the query. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns>
        ///     A Task.
        /// </returns>
        public virtual async Task RegisterValueBufferCursorAsync(
            [NotNull] IValueBufferCursor valueBufferCursor, int? queryIndex, CancellationToken cancellationToken)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            if (Connection.ActiveCursor != null
                && !Connection.IsMultipleActiveResultSetsEnabled)
            {
                await Connection.ActiveCursor.BufferAllAsync(cancellationToken);
            }

            Connection.ActiveCursor = valueBufferCursor;

            _activeQueries.Add(valueBufferCursor);

            if (queryIndex.HasValue
                && queryIndex.Value > 0)
            {
                AddBufferCursorToIncludeQueriesList(valueBufferCursor, queryIndex.Value);
            }
        }

        private void AddBufferCursorToIncludeQueriesList(IValueBufferCursor valueBufferCursor, int includeQueryIndex)
        {
            if (includeQueryIndex > _activeIncludeQueries.Count)
            {
                var missingEntries = includeQueryIndex - _activeIncludeQueries.Count;

                for (var i = 0; i < missingEntries; i++)
                {
                    _activeIncludeQueries.Add(null);
                }
            }

            _activeIncludeQueries[includeQueryIndex - 1] = valueBufferCursor;
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

            var index = _activeIncludeQueries.IndexOf(valueBufferCursor);

            if (index >= 0)
            {
                _activeIncludeQueries[index] = null;
            }
        }

        /// <summary>
        ///     Gets the include value buffer for a given query index.
        /// </summary>
        /// <param name="queryIndex"> Zero-based index of the query. </param>
        /// <returns>
        ///     The include value buffer.
        /// </returns>
        public virtual ValueBuffer GetIncludeValueBuffer(int queryIndex)
            => queryIndex == 0
                ? _activeQueries[_activeIncludeQueryOffset + queryIndex].Current
                : _activeIncludeQueries[queryIndex - 1].Current;

        /// <summary>
        ///     Begins an include scope.
        /// </summary>
        public virtual void BeginIncludeScope() => _activeIncludeQueryOffset = _activeQueries.Count;

        /// <summary>
        ///     Ends an include scope.
        /// </summary>
        public virtual void EndIncludeScope()
        {
            for (var i = _activeQueries.Count - 1; i > _activeIncludeQueryOffset; i--)
            {
                _activeQueries.RemoveAt(i);
            }

            _activeIncludeQueries.Clear();

            _activeIncludeQueryOffset = 0;
        }
    }
}
