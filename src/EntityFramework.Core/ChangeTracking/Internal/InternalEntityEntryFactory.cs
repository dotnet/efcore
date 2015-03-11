// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class InternalEntityEntryFactory
    {
        private readonly EntityEntryMetadataServices _metadataServices;

        public InternalEntityEntryFactory([NotNull] EntityEntryMetadataServices metadataServices)
        {
            _metadataServices = metadataServices;
        }

        public virtual InternalEntityEntry Create(
            [NotNull] StateManager stateManager,
            [NotNull] IEntityType entityType,
            [CanBeNull] object entity)
            => NewInternalEntityEntry(stateManager, entityType, entity);

        public virtual InternalEntityEntry Create(
            [NotNull] StateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            [NotNull] IValueReader valueReader)
            => NewInternalEntityEntry(stateManager, entityType, entity, valueReader);

        private InternalEntityEntry NewInternalEntityEntry(StateManager stateManager, IEntityType entityType, object entity)
        {
            if (!entityType.HasClrType)
            {
                return new InternalShadowEntityEntry(stateManager, entityType, _metadataServices);
            }

            Debug.Assert(entity != null);

            return entityType.ShadowPropertyCount > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, _metadataServices, entity)
                : new InternalClrEntityEntry(stateManager, entityType, _metadataServices, entity);
        }

        private InternalEntityEntry NewInternalEntityEntry(StateManager stateManager, IEntityType entityType, object entity, IValueReader valueReader)
        {
            if (!entityType.HasClrType)
            {
                return new InternalShadowEntityEntry(stateManager, entityType, _metadataServices, valueReader);
            }

            return entityType.ShadowPropertyCount > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, _metadataServices, entity, valueReader)
                : new InternalClrEntityEntry(stateManager, entityType, _metadataServices, entity);
        }
    }
}
