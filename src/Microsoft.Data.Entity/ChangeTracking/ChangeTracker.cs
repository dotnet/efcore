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
        private readonly Dictionary<EntityKey, ChangeTrackerEntry> _identityMap = new Dictionary<EntityKey, ChangeTrackerEntry>();

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
        }

        public virtual EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity)
        {
            Check.NotNull(entity, "entity");

            var entityType = GetEntityType(entity);
            var entry = TryGetEntry(entityType, entity);
            return entry != null ? new EntityEntry<TEntity>(entry) : new EntityEntry<TEntity>(this, entity);
        }

        public virtual EntityEntry Entry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            var entityType = GetEntityType(entity);
            var entry = TryGetEntry(entityType, entity);
            return entry != null ? new EntityEntry(entry) : new EntityEntry(this, entity);
        }

        private IEntityType GetEntityType(object entity)
        {
            // TODO: Consider what to do with derived types that are not explicitly in the model

            var entityType = _model.EntityType(entity);

            if (entityType == null)
            {
                // TODO: Consider specialized exception types
                throw new InvalidOperationException(Strings.TypeNotInModel(entity.GetType().Name));
            }

            return entityType;
        }

        private ChangeTrackerEntry TryGetEntry(IEntityType entityType, object entity)
        {
            var key = entityType.CreateEntityKey(entity);

            ChangeTrackerEntry entry;
            return _identityMap.TryGetValue(key, out entry) && ReferenceEquals(entry.Entity, entity) ? entry : null;
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
            if (_identityMap.TryGetValue(entry.Key, out existingEntry)
                && !ReferenceEquals(entry.Entity, existingEntry.Entity))
            {
                // TODO: Consider a hook for identity resolution
                // TODO: Consider specialized exception types
                throw new InvalidOperationException(Strings.IdentityConflict(entry.Entity.GetType().Name));
            }

            // TODO: Consider the case where two EntityEntry instances both track the same entity instance
            _identityMap[entry.Key] = entry;
        }

        internal virtual void StopTracking(ChangeTrackerEntry entry)
        {
            var key = entry.Key;
            if (_identityMap.ContainsKey(key))
            {
                _identityMap.Remove(key);
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
