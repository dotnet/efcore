// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <inheritdoc />
    public class OriginalValuesFactoryFactory : SnapshotFactoryFactory<InternalEntityEntry>
    {
        /// <inheritdoc />
        protected override int GetPropertyIndex(IPropertyBase propertyBase)
            => propertyBase.GetOriginalValueIndex();

        /// <inheritdoc />
        protected override int GetPropertyCount(IEntityType entityType)
            => entityType.OriginalValueCount();

        /// <inheritdoc />
        protected override ValueComparer GetValueComparer(IProperty property)
            => property.GetValueComparer();
    }
}
