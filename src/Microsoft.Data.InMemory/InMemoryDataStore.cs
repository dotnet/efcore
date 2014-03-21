// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStore : DataStore
    {
        private readonly ThreadSafeLazyRef<ImmutableDictionary<EntityKey, object[]>> _objectData
            = new ThreadSafeLazyRef<ImmutableDictionary<EntityKey, object[]>>(() => ImmutableDictionary<EntityKey, object[]>.Empty);

        private readonly ILogger _logger;

        public InMemoryDataStore()
            : this(NullLogger.Instance)
        {
        }

        public InMemoryDataStore([NotNull] ILoggerFactory loggerFactory)
            : this(Check.NotNull(loggerFactory, "loggerFactory").Create(typeof(InMemoryDataStore).Name))
        {
        }

        public InMemoryDataStore([NotNull] ILogger logger)
        {
            Check.NotNull(logger, "logger");

            _logger = logger;
        }

        public override Task<int> SaveChangesAsync(
            IEnumerable<StateEntry> stateEntries, IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");
            Check.NotNull(model, "model");

            var added = new List<StateEntry>();
            var deleted = new List<StateEntry>();
            var modified = new List<StateEntry>();

            foreach (var stateEntry in stateEntries)
            {
                switch (stateEntry.EntityState)
                {
                    case EntityState.Added:
                        added.Add(stateEntry);
                        break;

                    case EntityState.Deleted:
                        deleted.Add(stateEntry);
                        break;

                    case EntityState.Modified:
                        modified.Add(stateEntry);
                        break;
                }
            }

            _objectData.ExchangeValue(
                db => db.AddRange(added.Select(se => new KeyValuePair<EntityKey, object[]>(se.GetPrimaryKeyValue(), se.GetValueBuffer())))
                    .SetItems(modified.Select(se => new KeyValuePair<EntityKey, object[]>(se.GetPrimaryKeyValue(), se.GetValueBuffer())))
                    .RemoveRange(deleted.Select(se => se.GetPrimaryKeyValue())));

            if (_logger.IsEnabled(TraceType.Information))
            {
                foreach (var stateEntry in added.Concat(modified).Concat(deleted))
                {
                    _logger.WriteInformation(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Saved entity of type: '{0}' [{1}]",
                            stateEntry.EntityType.Name,
                            stateEntry.EntityState));
                }
            }

            return Task.FromResult(added.Count + modified.Count + deleted.Count);
        }

        public override IAsyncEnumerable<object[]> Read(IEntityType entityType)
        {
            return new CompletedAsyncEnumerable(
                _objectData.Value
                    .Where(kv => kv.Key.EntityType == entityType)
                    .Select(kv => kv.Value));
        }

        internal IDictionary<EntityKey, object[]> Objects
        {
            get { return _objectData.Value; }
        }

        private sealed class CompletedAsyncEnumerable : IAsyncEnumerable<object[]>
        {
            private readonly IEnumerable<object[]> _enumerable;

            public CompletedAsyncEnumerable(IEnumerable<object[]> enumerable)
            {
                _enumerable = enumerable;
            }

            public IAsyncEnumerator<object[]> GetAsyncEnumerator()
            {
                return new CompletedAsyncEnumerator(_enumerable.GetEnumerator());
            }

            IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
            {
                return GetAsyncEnumerator();
            }
        }

        private sealed class CompletedAsyncEnumerator : IAsyncEnumerator<object[]>
        {
            private readonly IEnumerator<object[]> _enumerator;

            public CompletedAsyncEnumerator(IEnumerator<object[]> enumerator)
            {
                _enumerator = enumerator;
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(_enumerator.MoveNext());
            }

            public object[] Current
            {
                get { return _enumerator.Current; }
            }

            object IAsyncEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }
        }
    }
}
