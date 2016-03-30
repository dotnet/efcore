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
    public class RelationalQueryContext : QueryContext
    {
        private readonly List<IValueBufferCursor> _activeQueries = new List<IValueBufferCursor>();
        private readonly List<IValueBufferCursor> _activeIncludeQueries = new List<IValueBufferCursor>();

        private int _activeIncludeQueryOffset;

        public RelationalQueryContext(
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IRelationalConnection connection,
            [NotNull] IStateManager stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector)
            : base(
                Check.NotNull(queryBufferFactory, nameof(queryBufferFactory)),
                Check.NotNull(stateManager, nameof(stateManager)),
                Check.NotNull(concurrencyDetector, nameof(concurrencyDetector)))
        {
            Check.NotNull(connection, nameof(connection));

            Connection = connection;
        }

        public virtual IRelationalConnection Connection { get; }

        public virtual SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

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

        public virtual ValueBuffer GetIncludeValueBuffer(int queryIndex)
            => queryIndex == 0
                ? _activeQueries[_activeIncludeQueryOffset + queryIndex].Current
                : _activeIncludeQueries[queryIndex - 1].Current;

        public virtual void BeginIncludeScope() => _activeIncludeQueryOffset = _activeQueries.Count;

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
