// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntryMetadataServices
    {
        private readonly ClrPropertyGetterSource _getterSource;
        private readonly ClrPropertySetterSource _setterSource;
        private readonly OriginalValuesFactory _originalValuesFactory;
        private readonly RelationshipsSnapshotFactory _relationshipsSnapshotFactory;
        private readonly StoreGeneratedValuesFactory _storeGeneratedValuesFactory;
        private readonly EntityKeyFactorySource _entityKeyFactorySource;

        public StateEntryMetadataServices(
            [NotNull] ClrPropertyGetterSource getterSource,
            [NotNull] ClrPropertySetterSource setterSource,
            [NotNull] OriginalValuesFactory originalValuesFactory,
            [NotNull] RelationshipsSnapshotFactory relationshipsSnapshotFactory,
            [NotNull] StoreGeneratedValuesFactory storeGeneratedValuesFactory,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource)
        {
            Check.NotNull(getterSource, "getterSource");
            Check.NotNull(setterSource, "setterSource");
            Check.NotNull(originalValuesFactory, "originalValuesFactory");
            Check.NotNull(relationshipsSnapshotFactory, "relationshipsSnapshotFactory");
            Check.NotNull(storeGeneratedValuesFactory, "storeGeneratedValuesFactory");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");

            _getterSource = getterSource;
            _setterSource = setterSource;
            _originalValuesFactory = originalValuesFactory;
            _relationshipsSnapshotFactory = relationshipsSnapshotFactory;
            _storeGeneratedValuesFactory = storeGeneratedValuesFactory;
            _entityKeyFactorySource = entityKeyFactorySource;
        }

        public virtual object ReadValue([NotNull] object entity, [NotNull] IPropertyBase propertyBase)
        {
            Check.NotNull(entity, "entity");
            Check.NotNull(propertyBase, "propertyBase");

            return _getterSource.GetAccessor(propertyBase).GetClrValue(entity);
        }

        public virtual void WriteValue([NotNull] object entity, [NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            Check.NotNull(entity, "entity");
            Check.NotNull(propertyBase, "propertyBase");

            _setterSource.GetAccessor(propertyBase).SetClrValue(entity, value);
        }

        public virtual Sidecar CreateOriginalValues([NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");

            return _originalValuesFactory.Create(stateEntry);
        }

        public virtual Sidecar CreateRelationshipSnapshot([NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");

            return _relationshipsSnapshotFactory.Create(stateEntry);
        }

        public virtual Sidecar CreateStoreGeneratedValues([NotNull] StateEntry stateEntry, [NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(properties, "properties");

            return _storeGeneratedValuesFactory.Create(stateEntry, properties);
        }

        public virtual EntityKey CreateKey(
            [NotNull] IEntityType entityType,
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IPropertyBagEntry propertyBagEntry)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotEmpty(properties, "properties");
            Check.NotNull(propertyBagEntry, "propertyBagEntry");

            return _entityKeyFactorySource
                .GetKeyFactory(properties)
                .Create(entityType, properties, propertyBagEntry);
        }
    }
}
