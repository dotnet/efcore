// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityEntryMetadataServices : IEntityEntryMetadataServices
    {
        private readonly IClrAccessorSource<IClrPropertyGetter> _getterSource;
        private readonly IClrAccessorSource<IClrPropertySetter> _setterSource;
        private readonly IOriginalValuesFactory _originalValuesFactory;
        private readonly IRelationshipsSnapshotFactory _relationshipsSnapshotFactory;
        private readonly IStoreGeneratedValuesFactory _storeGeneratedValuesFactory;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;

        public EntityEntryMetadataServices(
            [NotNull] IClrAccessorSource<IClrPropertyGetter> getterSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> setterSource,
            [NotNull] IOriginalValuesFactory originalValuesFactory,
            [NotNull] IRelationshipsSnapshotFactory relationshipsSnapshotFactory,
            [NotNull] IStoreGeneratedValuesFactory storeGeneratedValuesFactory,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource)
        {
            _getterSource = getterSource;
            _setterSource = setterSource;
            _originalValuesFactory = originalValuesFactory;
            _relationshipsSnapshotFactory = relationshipsSnapshotFactory;
            _storeGeneratedValuesFactory = storeGeneratedValuesFactory;
            _entityKeyFactorySource = entityKeyFactorySource;
        }

        public virtual object ReadValue(object entity, IPropertyBase propertyBase)
            => _getterSource.GetAccessor(propertyBase).GetClrValue(entity);

        public virtual void WriteValue(object entity, IPropertyBase propertyBase, object value)
            => _setterSource.GetAccessor(propertyBase).SetClrValue(entity, value);

        public virtual Sidecar CreateOriginalValues(InternalEntityEntry entry)
            => _originalValuesFactory.Create(entry);

        public virtual Sidecar CreateRelationshipSnapshot(InternalEntityEntry entry)
            => _relationshipsSnapshotFactory.Create(entry);

        public virtual Sidecar CreateStoreGeneratedValues(InternalEntityEntry entry, IReadOnlyList<IProperty> properties)
            => _storeGeneratedValuesFactory.Create(entry, properties);

        public virtual EntityKey CreateKey(IEntityType entityType, IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
            => _entityKeyFactorySource
                .GetKeyFactory(properties)
                .Create(entityType, properties, propertyAccessor);
    }
}
