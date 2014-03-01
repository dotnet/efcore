// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // This is the app-developer facing public API to the change tracker
    public class ChangeTracker
    {
        private readonly StateManager _stateManager;

        // Intended only for creation of test doubles
        internal ChangeTracker()
        {
        }

        public ChangeTracker([NotNull] StateManager stateManager)
        {
            Check.NotNull(stateManager, "stateManager");

            _stateManager = stateManager;
        }

        public virtual EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            return new EntityEntry<TEntity>(_stateManager.GetOrCreateEntry(entity));
        }

        public virtual EntityEntry Entry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            return new EntityEntry(_stateManager.GetOrCreateEntry(entity));
        }

        public virtual IEnumerable<EntityEntry> Entries()
        {
            return _stateManager.StateEntries.Select(e => new EntityEntry(e));
        }

        public virtual IEnumerable<EntityEntry<TEntity>> Entries<TEntity>()
        {
            return _stateManager.StateEntries
                .Where(e => e.Entity is TEntity)
                .Select(e => new EntityEntry<TEntity>(e));
        }

        public virtual StateManager StateManager
        {
            get { return _stateManager; }
        }
    }
}
