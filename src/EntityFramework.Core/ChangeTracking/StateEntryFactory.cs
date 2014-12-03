// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntryFactory
    {
        private readonly EntityMaterializerSource _materializerSource;
        private readonly StateEntryMetadataServices _metadataServices;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateEntryFactory()
        {
        }

        public StateEntryFactory(
            [NotNull] EntityMaterializerSource materializerSource,
            [NotNull] StateEntryMetadataServices metadataServices)
        {
            Check.NotNull(materializerSource, "materializerSource");
            Check.NotNull(metadataServices, "metadataServices");

            _materializerSource = materializerSource;
            _metadataServices = metadataServices;
        }

        public virtual StateEntry Create(
            [NotNull] StateManager stateManager,
            [NotNull] IEntityType entityType,
            [CanBeNull] object entity)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityType, "entityType");

            return NewStateEntry(stateManager, entityType, entity);
        }

        public virtual StateEntry Create(
            [NotNull] StateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] IValueReader valueReader)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            return NewStateEntry(stateManager, entityType, valueReader);
        }

        public virtual StateEntry Create(
            [NotNull] StateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            [NotNull] IValueReader valueReader)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityType, "entityType");
            Check.NotNull(entity, "entity");
            Check.NotNull(valueReader, "valueReader");

            return NewStateEntry(stateManager, entityType, entity, valueReader);
        }

        private StateEntry NewStateEntry(StateManager stateManager, IEntityType entityType, object entity)
        {
            if (!entityType.HasClrType)
            {
                return new ShadowStateEntry(stateManager, entityType, _metadataServices);
            }

            Check.NotNull(entity, "entity");

            return entityType.ShadowPropertyCount > 0
                ? (StateEntry)new MixedStateEntry(stateManager, entityType, _metadataServices, entity)
                : new ClrStateEntry(stateManager, entityType, _metadataServices, entity);
        }

        private StateEntry NewStateEntry(StateManager stateManager, IEntityType entityType, IValueReader valueReader)
        {
            if (!entityType.HasClrType)
            {
                return new ShadowStateEntry(stateManager, entityType, _metadataServices, valueReader);
            }

            var entity = _materializerSource.GetMaterializer(entityType)(valueReader);

            return entityType.ShadowPropertyCount > 0
                ? (StateEntry)new MixedStateEntry(stateManager, entityType, _metadataServices, entity, valueReader)
                : new ClrStateEntry(stateManager, entityType, _metadataServices, entity);
        }

        private StateEntry NewStateEntry(StateManager stateManager, IEntityType entityType, object entity, IValueReader valueReader)
        {
            if (!entityType.HasClrType)
            {
                return new ShadowStateEntry(stateManager, entityType, _metadataServices, valueReader);
            }

            return entityType.ShadowPropertyCount > 0
                ? (StateEntry)new MixedStateEntry(stateManager, entityType, _metadataServices, entity, valueReader)
                : new ClrStateEntry(stateManager, entityType, _metadataServices, entity);
        }
    }
}
