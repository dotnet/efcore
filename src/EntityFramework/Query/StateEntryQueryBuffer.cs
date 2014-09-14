// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class StateEntryQueryBuffer : IQueryBuffer
    {
        private readonly StateManager _stateManager;
        private readonly EntityKeyFactorySource _entityKeyFactorySource;
        private readonly StateEntryFactory _stateEntryFactory;

        private readonly Dictionary<EntityKey, StateEntry> _stateEntriesByEntityKey
            = new Dictionary<EntityKey, StateEntry>();

        private readonly Dictionary<object, StateEntry> _stateEntriesByEntityInstance
            = new Dictionary<object, StateEntry>(ReferenceEqualityComparer.Instance);

        public StateEntryQueryBuffer(
            [NotNull] StateManager stateManager,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] StateEntryFactory stateEntryFactory)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(stateEntryFactory, "stateEntryFactory");

            _stateManager = stateManager;
            _entityKeyFactorySource = entityKeyFactorySource;
            _stateEntryFactory = stateEntryFactory;
        }

        public virtual object GetEntity(IEntityType entityType, IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            var keyProperties = entityType.GetPrimaryKey().Properties;

            var entityKey
                = _entityKeyFactorySource
                    .GetKeyFactory(keyProperties)
                    .Create(entityType, keyProperties, valueReader);

            var stateEntry = _stateManager.TryGetEntry(entityKey);

            if (stateEntry == null
                && !_stateEntriesByEntityKey.TryGetValue(entityKey, out stateEntry))
            {
                stateEntry = _stateEntryFactory.Create(entityType, valueReader);

                _stateEntriesByEntityKey.Add(entityKey, stateEntry);
                _stateEntriesByEntityInstance.Add(stateEntry.Entity, stateEntry);
            }

            return stateEntry.Entity;
        }

        public virtual StateEntry TryGetStateEntry(object entity)
        {
            Check.NotNull(entity, "entity");

            StateEntry stateEntry;

            return _stateManager.TryGetEntry(entity)
                   ?? (_stateEntriesByEntityInstance.TryGetValue(entity, out stateEntry)
                       ? stateEntry
                       : null);
        }
    }
}
