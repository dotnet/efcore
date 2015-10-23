// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityEntryMetadataServices : IEntityEntryMetadataServices
    {
        private readonly IOriginalValuesFactory _originalValuesFactory;
        private readonly IRelationshipsSnapshotFactory _relationshipsSnapshotFactory;
        private readonly IStoreGeneratedValuesFactory _storeGeneratedValuesFactory;
        private readonly IKeyValueFactorySource _keyValueFactorySource;

        public EntityEntryMetadataServices(
            [NotNull] IOriginalValuesFactory originalValuesFactory,
            [NotNull] IRelationshipsSnapshotFactory relationshipsSnapshotFactory,
            [NotNull] IStoreGeneratedValuesFactory storeGeneratedValuesFactory,
            [NotNull] IKeyValueFactorySource keyValueFactorySource)
        {
            _originalValuesFactory = originalValuesFactory;
            _relationshipsSnapshotFactory = relationshipsSnapshotFactory;
            _storeGeneratedValuesFactory = storeGeneratedValuesFactory;
            _keyValueFactorySource = keyValueFactorySource;
        }

        public virtual Sidecar CreateOriginalValues(InternalEntityEntry entry)
            => _originalValuesFactory.Create(entry);

        public virtual Sidecar CreateRelationshipSnapshot(InternalEntityEntry entry)
            => _relationshipsSnapshotFactory.Create(entry);

        public virtual Sidecar CreateStoreGeneratedValues(InternalEntityEntry entry, IReadOnlyList<IProperty> properties)
            => _storeGeneratedValuesFactory.Create(entry, properties);

        public virtual IKeyValue CreateKey(IKey key, IReadOnlyList<IProperty> properties, IPropertyAccessor propertyAccessor)
            => _keyValueFactorySource
                .GetKeyFactory(key)
                .Create(properties, propertyAccessor);
    }
}
