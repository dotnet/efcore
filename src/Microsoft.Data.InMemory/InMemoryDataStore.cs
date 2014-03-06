// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStore : DataStore
    {
        private readonly ThreadSafeLazyRef<ImmutableDictionary<object, object[]>> _objectData
            = new ThreadSafeLazyRef<ImmutableDictionary<object, object[]>>(() => ImmutableDictionary<object, object[]>.Empty);

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

        public override Task<int> SaveChangesAsync(IEnumerable<StateEntry> stateEntries, IModel model)
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
                db => db.WithComparers(model.EntityEqualityComparer)
                    .AddRange(added.Select(se => new KeyValuePair<object, object[]>(se.Entity, se.GetValueBuffer())))
                    .SetItems(modified.Select(se => new KeyValuePair<object, object[]>(se.Entity, se.GetValueBuffer())))
                    .RemoveRange(deleted.Select(se => se.Entity)));

            if (_logger.IsEnabled(TraceType.Information))
            {
                foreach (var stateEntry in added.Concat(modified).Concat(deleted))
                {
                    _logger.WriteInformation(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Saved entity of type: '{0}' [{1}]",
                            stateEntry.Entity.GetType().Name,
                            stateEntry.EntityState));
                }
            }

            return Task.FromResult(added.Count + modified.Count + deleted.Count);
        }

        public virtual IDictionary<object, object[]> Objects
        {
            get { return _objectData.Value; }
        }
    }
}
