// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityEntryMetadataServices
    {
        private readonly ClrPropertyGetterSource _getterSource;
        private readonly ClrPropertySetterSource _setterSource;
        private readonly OriginalValuesFactory _originalValuesFactory;
        private readonly RelationshipsSnapshotFactory _relationshipsSnapshotFactory;
        private readonly StoreGeneratedValuesFactory _storeGeneratedValuesFactory;
        private readonly EntityKeyFactorySource _entityKeyFactorySource;

        public EntityEntryMetadataServices(
            [NotNull] ClrPropertyGetterSource getterSource,
            [NotNull] ClrPropertySetterSource setterSource,
            [NotNull] OriginalValuesFactory originalValuesFactory,
            [NotNull] RelationshipsSnapshotFactory relationshipsSnapshotFactory,
            [NotNull] StoreGeneratedValuesFactory storeGeneratedValuesFactory,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource)
        {
            _getterSource = getterSource;
            _setterSource = setterSource;
            _originalValuesFactory = originalValuesFactory;
            _relationshipsSnapshotFactory = relationshipsSnapshotFactory;
            _storeGeneratedValuesFactory = storeGeneratedValuesFactory;
            _entityKeyFactorySource = entityKeyFactorySource;
        }

        public virtual object ReadValue([NotNull] object entity, [NotNull] IPropertyBase propertyBase)
            => _getterSource.GetAccessor(propertyBase).GetClrValue(entity);

        public virtual void WriteValue([NotNull] object entity, [NotNull] IPropertyBase propertyBase, [CanBeNull] object value) 
            => _setterSource.GetAccessor(propertyBase).SetClrValue(entity, value);

        public virtual Sidecar CreateOriginalValues([NotNull] InternalEntityEntry entry)
            => _originalValuesFactory.Create(entry);

        public virtual Sidecar CreateRelationshipSnapshot([NotNull] InternalEntityEntry entry)
            => _relationshipsSnapshotFactory.Create(entry);

        public virtual Sidecar CreateStoreGeneratedValues([NotNull] InternalEntityEntry entry, [NotNull] IReadOnlyList<IProperty> properties)
            => _storeGeneratedValuesFactory.Create(entry, properties);

        public virtual EntityKey CreateKey(
            [NotNull] IEntityType entityType,
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IPropertyAccessor propertyAccessor)
            => _entityKeyFactorySource
                .GetKeyFactory(properties)
                .Create(entityType, properties, propertyAccessor);
    }
}
