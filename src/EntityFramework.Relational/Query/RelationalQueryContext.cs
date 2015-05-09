// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryContext : QueryContext
    {
        private readonly List<IValueBufferCursor> _activeQueries = new List<IValueBufferCursor>();

        private int _activeQueryOffset;

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

        public virtual void RegisterActiveQuery([NotNull] IValueBufferCursor valueBufferCursor)
        {
            Check.NotNull(valueBufferCursor, nameof(valueBufferCursor));

            _activeQueries.Add(valueBufferCursor);
        }

        public virtual ValueBuffer GetValueBuffer(int queryIndex)
            => _activeQueries[_activeQueryOffset + queryIndex].Current;

        public virtual void BeginIncludeScope() => _activeQueryOffset = _activeQueries.Count;

        public virtual void EndIncludeScope()
        {
            for (var i = _activeQueries.Count - 1; i > _activeQueryOffset; i--)
            {
                _activeQueries.RemoveAt(i);
            }

            _activeQueryOffset = 0;
        }
    }
}
