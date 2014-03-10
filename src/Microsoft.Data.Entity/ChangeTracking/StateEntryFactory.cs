// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntryFactory
    {
        public virtual StateEntry Create(
            [NotNull] StateManager stateManager, [NotNull] IEntityType entityType, [CanBeNull] object entity)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityType, "entityType");

            if (!entityType.HasClrType)
            {
                return new ShadowStateEntry(stateManager, entityType);
            }

            Check.NotNull(entity, "entity");

            return entityType.ShadowPropertyCount > 0
                ? (StateEntry)new MixedStateEntry(stateManager, entityType, entity)
                : new ClrStateEntry(stateManager, entityType, entity);
        }
    }
}
