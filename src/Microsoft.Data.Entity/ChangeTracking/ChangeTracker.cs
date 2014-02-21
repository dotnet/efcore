// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeTracker
    {
        private readonly IModel _model;
        private readonly Dictionary<EntityKey, ChangeTrackerEntry> _identityMap = new Dictionary<EntityKey, ChangeTrackerEntry>();

        public ChangeTracker([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _model = model;
        }

        public virtual EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            var impl = TryGetEntry(entity);
            return impl != null ? new EntityEntry<TEntity>(impl) : new EntityEntry<TEntity>(this, entity);
        }

        public virtual EntityEntry Entry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            var impl = TryGetEntry(entity);
            return impl != null ? new EntityEntry(impl) : new EntityEntry(this, entity);
        }

        private ChangeTrackerEntry TryGetEntry(object entity)
        {
            // TODO: Error checking for type that is not in the model
            // TODO: Error checking for different entity instance with same key

            var key = _model.Entity(entity).CreateEntityKey(entity);

            ChangeTrackerEntry entry;
            return _identityMap.TryGetValue(key, out entry) ? entry : null;
        }

        public virtual IEnumerable<EntityEntry> Entries()
        {
            return _identityMap.Values.Select(e => new EntityEntry(e));
        }

        public virtual IEnumerable<EntityEntry<TEntity>> Entries<TEntity>()
        {
            return _identityMap.Values
                .Where(e => e.Entity is TEntity)
                .Select(e => new EntityEntry<TEntity>(e));
        }

        internal void Track(ChangeTrackerEntry entry)
        {
            // TODO: Error checking for entry/entity that is already tracked

            _identityMap[entry.Key] = entry;
        }

        internal void Detach(ChangeTrackerEntry entry)
        {
            // TODO: Error checking for entry/entity that is not being tracked

            _identityMap.Remove(entry.Key);
        }

        public virtual IModel Model
        {
            get { return _model; }
        }
    }
}
