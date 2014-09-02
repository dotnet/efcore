// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class StateManagerMaterializationStrategy : IMaterializationStrategy
    {
        private readonly StateManager _stateManager;

        public StateManagerMaterializationStrategy([NotNull] StateManager stateManager)
        {
            Check.NotNull(stateManager, "stateManager");

            _stateManager = stateManager;
        }

        public virtual object Materialize(IEntityType entityType, IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            return _stateManager.GetOrMaterializeEntry(entityType, valueReader).Entity;
        }

        public virtual object GetPropertyValue(object entity, IProperty property)
        {
            Check.NotNull(entity, "entity");
            Check.NotNull(property, "property");

            return _stateManager.GetOrCreateEntry(entity)[property];
        }
    }
}
