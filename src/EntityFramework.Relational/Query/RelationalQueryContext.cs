// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalQueryContext : QueryContext
    {
        private readonly List<IValueBufferCursor> _activeQueries = new List<IValueBufferCursor>();

        private int _activeIncludeQueryOffset;

        public RelationalQueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] IRelationalConnection connection)
            : base(
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(queryBuffer, nameof(queryBuffer)))
        {
            Check.NotNull(connection, nameof(connection));

            Connection = connection;
        }

        public virtual IRelationalConnection Connection { get; }

        public virtual void RegisterValueBufferCursor([NotNull] IValueBufferCursor valueBufferCursor)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            if (!Connection.IsMultipleActiveResultSetsEnabled
                && _activeQueries.Count > 0)
            {
                _activeQueries.Last().BufferAll();
            }

            _activeQueries.Add(valueBufferCursor);
        }

        public virtual async Task RegisterValueBufferCursorAsync(
            [NotNull] IValueBufferCursor valueBufferCursor, CancellationToken cancellationToken)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            if (!Connection.IsMultipleActiveResultSetsEnabled
                && _activeQueries.Count > 0)
            {
                await _activeQueries.Last().BufferAllAsync(cancellationToken);
            }

            _activeQueries.Add(valueBufferCursor);
        }

        public virtual void DeregisterValueBufferCursor([NotNull] IValueBufferCursor valueBufferCursor)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            _activeQueries.Remove(valueBufferCursor);
        }

        public virtual ValueBuffer GetIncludeValueBuffer(int queryIndex)
            => _activeQueries[_activeIncludeQueryOffset + queryIndex].Current;

        public virtual void BeginIncludeScope() => _activeIncludeQueryOffset = _activeQueries.Count;

        public virtual void EndIncludeScope()
        {
            for (var i = _activeQueries.Count - 1; i > _activeIncludeQueryOffset; i--)
            {
                _activeQueries.RemoveAt(i);
            }

            _activeIncludeQueryOffset = 0;
        }
    }
}
