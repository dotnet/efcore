// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <inheritdoc />
    public class RelationshipSnapshotFactoryFactory : SnapshotFactoryFactory<InternalEntityEntry>
    {
        /// <inheritdoc />
        protected override int GetPropertyIndex(IPropertyBase propertyBase)
            => propertyBase.GetRelationshipIndex();

        /// <inheritdoc />
        protected override int GetPropertyCount(IEntityType entityType)
            => entityType.RelationshipPropertyCount();

        /// <inheritdoc />
        protected override ValueComparer GetValueComparer(IProperty property)
            => property.GetKeyValueComparer();
    }
}
