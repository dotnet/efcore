// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalEntityEntryFactory : IInternalEntityEntryFactory
    {
        public virtual InternalEntityEntry Create(IStateManager stateManager, IEntityType entityType, object entity)
            => NewInternalEntityEntry(stateManager, entityType, entity);

        public virtual InternalEntityEntry Create(IStateManager stateManager, IEntityType entityType, object entity, ValueBuffer valueBuffer)
            => NewInternalEntityEntry(stateManager, entityType, entity, valueBuffer);

        private static InternalEntityEntry NewInternalEntityEntry(IStateManager stateManager, IEntityType entityType, object entity)
        {
            if (!entityType.HasClrType())
            {
                return new InternalShadowEntityEntry(stateManager, entityType);
            }

            Debug.Assert(entity != null);

            return entityType.ShadowPropertyCount() > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, entity)
                : new InternalClrEntityEntry(stateManager, entityType, entity);
        }

        private static InternalEntityEntry NewInternalEntityEntry(
            IStateManager stateManager,
            IEntityType entityType,
            object entity,
            ValueBuffer valueBuffer)
        {
            if (!entityType.HasClrType())
            {
                return new InternalShadowEntityEntry(stateManager, entityType, valueBuffer);
            }

            return entityType.ShadowPropertyCount() > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, entity, valueBuffer)
                : new InternalClrEntityEntry(stateManager, entityType, entity);
        }
    }
}
