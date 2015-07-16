// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class InternalEntityEntryFactory : IInternalEntityEntryFactory
    {
        private readonly IEntityEntryMetadataServices _metadataServices;

        public InternalEntityEntryFactory([NotNull] IEntityEntryMetadataServices metadataServices)
        {
            _metadataServices = metadataServices;
        }

        public virtual InternalEntityEntry Create(IStateManager stateManager, IEntityType entityType, object entity)
            => NewInternalEntityEntry(stateManager, entityType, entity);

        public virtual InternalEntityEntry Create(IStateManager stateManager, IEntityType entityType, object entity, ValueBuffer valueBuffer)
            => NewInternalEntityEntry(stateManager, entityType, entity, valueBuffer);

        private InternalEntityEntry NewInternalEntityEntry(IStateManager stateManager, IEntityType entityType, object entity)
        {
            if (!entityType.HasClrType())
            {
                return new InternalShadowEntityEntry(stateManager, entityType, _metadataServices);
            }

            Debug.Assert(entity != null);

            return entityType.ShadowPropertyCount() > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, _metadataServices, entity)
                : new InternalClrEntityEntry(stateManager, entityType, _metadataServices, entity);
        }

        private InternalEntityEntry NewInternalEntityEntry(
            IStateManager stateManager,
            IEntityType entityType,
            object entity,
            ValueBuffer valueBuffer)
        {
            if (!entityType.HasClrType())
            {
                return new InternalShadowEntityEntry(stateManager, entityType, _metadataServices, valueBuffer);
            }

            return entityType.ShadowPropertyCount() > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, _metadataServices, entity, valueBuffer)
                : new InternalClrEntityEntry(stateManager, entityType, _metadataServices, entity);
        }
    }
}
