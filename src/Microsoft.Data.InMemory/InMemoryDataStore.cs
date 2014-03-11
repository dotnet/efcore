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
            IEnumerable<StateEntry> stateEntries, IModel model, CancellationToken cancellationToken)
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
                db => db.AddRange(added.Select(se => new KeyValuePair<EntityKey, object[]>(se.CreateKey(), se.GetValueBuffer())))
                    .SetItems(modified.Select(se => new KeyValuePair<EntityKey, object[]>(se.CreateKey(), se.GetValueBuffer())))
                    .RemoveRange(deleted.Select(se => se.CreateKey())));

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
            return new CompletedAsyncEnumerable<object[]>(
                _objectData.Value
                    .Where(kv => kv.Key.EntityType == entityType)
                    .Select(kv => kv.Value));
        }

        internal IDictionary<EntityKey, object[]> Objects
        {
            get { return _objectData.Value; }
        }

        private class CompletedAsyncEnumerable<T> : IAsyncEnumerable<T>
            where T : class
        {
            private readonly IEnumerable<T> _enumerable;

            public CompletedAsyncEnumerable(IEnumerable<T> enumerable)
            {
                _enumerable = enumerable;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator()
            {
                return new CompletedAsyncEnumerator<T>(_enumerable.GetEnumerator());
            }

            IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
            {
                return GetAsyncEnumerator();
            }
        }

        private class CompletedAsyncEnumerator<T> : IAsyncEnumerator<T>
            where T : class
        {
            private readonly IEnumerator<T> _enumerator;

            public CompletedAsyncEnumerator(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_enumerator.MoveNext());
            }

            public T Current
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
