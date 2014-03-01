// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStore : DataStore
    {
        private readonly LazyRef<ImmutableDictionary<object, object[]>> _objectData
            = new LazyRef<ImmutableDictionary<object, object[]>>(() => ImmutableDictionary<object, object[]>.Empty);

        public override Task<int> SaveChangesAsync(IEnumerable<StateEntry> changeTrackerEntries, IModel model)
        {
            Check.NotNull(changeTrackerEntries, "changeTrackerEntries");
            Check.NotNull(model, "model");

            var added = new List<StateEntry>();
            var deleted = new List<StateEntry>();
            var modified = new List<StateEntry>();

            foreach (var changeTrackerEntry in changeTrackerEntries)
            {
                switch (changeTrackerEntry.EntityState)
                {
                    case EntityState.Added:
                        added.Add(changeTrackerEntry);
                        break;

                    case EntityState.Deleted:
                        deleted.Add(changeTrackerEntry);
                        break;

                    case EntityState.Modified:
                        modified.Add(changeTrackerEntry);
                        break;
                }
            }

            _objectData.ExchangeValue(
                db => db.WithComparers(model.EntityEqualityComparer)
                    .AddRange(added.Select(cte => new KeyValuePair<object, object[]>(cte.Entity, cte.GetValueBuffer())))
                    .SetItems(modified.Select(cte => new KeyValuePair<object, object[]>(cte.Entity, cte.GetValueBuffer())))
                    .RemoveRange(deleted.Select(cte => cte.Entity)));

            return Task.FromResult(added.Count + modified.Count + deleted.Count);
        }

        public virtual IDictionary<object, object[]> Objects
        {
            get { return _objectData.Value; }
        }
    }
}
