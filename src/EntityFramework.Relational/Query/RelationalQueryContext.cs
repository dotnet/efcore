// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryContext : QueryContext
    {
        private readonly List<DbDataReader> _activeDataReaders = new List<DbDataReader>();

        private int _activeReaderOffset;

        public RelationalQueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] IStateManager stateManager,
            [NotNull] IRelationalConnection connection,
            [NotNull] IRelationalValueReaderFactory valueReaderFactory)
            : base(
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(queryBuffer, nameof(queryBuffer)),
                Check.NotNull(stateManager, nameof(stateManager)))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(valueReaderFactory, nameof(valueReaderFactory));

            Connection = connection;
            ValueReaderFactory = valueReaderFactory;
        }

        // TODO: Move this to compilation context
        public virtual IRelationalValueReaderFactory ValueReaderFactory { get; }

        public virtual IRelationalConnection Connection { get; }

        public virtual void RegisterDataReader([NotNull] DbDataReader dataReader)
        {
            Check.NotNull(dataReader, nameof(dataReader));

            _activeDataReaders.Add(dataReader);
        }

        public virtual IValueReader CreateValueReader(int readerIndex)
            => ValueReaderFactory.CreateValueReader(_activeDataReaders[_activeReaderOffset + readerIndex]);

        public virtual void BeginIncludeScope() => _activeReaderOffset = _activeDataReaders.Count;

        public virtual void EndIncludeScope()
        {
            for (var i = _activeDataReaders.Count - 1; i > _activeReaderOffset; i--)
            {
                _activeDataReaders.RemoveAt(i);
            }

            _activeReaderOffset = 0;
        }
    }
}
