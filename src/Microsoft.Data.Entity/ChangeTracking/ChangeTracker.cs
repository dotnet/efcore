// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeTracker
    {
        private readonly IModel _model;
        private readonly ActiveIdentityGenerators _identityGenerators;
        private readonly Dictionary<object, ChangeTrackerEntry> _identityMap;

        // Intended only for creation of test doubles
        internal ChangeTracker()
        {
        }

        public ChangeTracker([NotNull] IModel model, [NotNull] ActiveIdentityGenerators identityGenerators)
        {
            Check.NotNull(model, "model");
            Check.NotNull(identityGenerators, "identityGenerators");

            _model = model;
            _identityGenerators = identityGenerators;
            _identityMap = new Dictionary<object, ChangeTrackerEntry>(_model.EntityEqualityComparer);
        }

        public virtual EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            var entry = TryGetEntry(entity);

            return entry != null
                ? new EntityEntry<TEntity>(entry)
                : new EntityEntry<TEntity>(this, entity);
        }

        public virtual EntityEntry Entry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            var entry = TryGetEntry(entity);

            return entry != null
                ? new EntityEntry(entry)
                : new EntityEntry(this, entity);
        }

        private ChangeTrackerEntry TryGetEntry(object entity)
        {
            ChangeTrackerEntry entry;
            return _identityMap.TryGetValue(entity, out entry)
                   && ReferenceEquals(entry.Entity, entity)
                ? entry
                : null;
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

        internal virtual void Track(ChangeTrackerEntry entry)
        {
            ChangeTrackerEntry existingEntry;
            if (_identityMap.TryGetValue(entry.Entity, out existingEntry)
                && !ReferenceEquals(entry.Entity, existingEntry.Entity))
            {
                // TODO: Consider a hook for identity resolution
                // TODO: Consider specialized exception types
                throw new InvalidOperationException(Strings.IdentityConflict(entry.Entity.GetType().Name));
            }

            // TODO: Consider the case where two EntityEntry instances both track the same entity instance
            _identityMap[entry.Entity] = entry;
        }

        internal virtual void StopTracking(ChangeTrackerEntry entry)
        {
            if (_identityMap.ContainsKey(entry.Entity))
            {
                _identityMap.Remove(entry.Entity);
            }
        }

        internal virtual IModel Model
        {
            get { return _model; }
        }

        internal virtual IIdentityGenerator GetIdentityGenerator(IProperty property)
        {
            return _identityGenerators.GetOrAdd(property);
        }
    }
}
