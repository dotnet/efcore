// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IEntityEntryMetadataServices
    {
        object ReadValue([NotNull] object entity, [NotNull] IPropertyBase propertyBase);
        void WriteValue([NotNull] object entity, [NotNull] IPropertyBase propertyBase, [CanBeNull] object value);
        Sidecar CreateOriginalValues([NotNull] InternalEntityEntry entry);
        Sidecar CreateRelationshipSnapshot([NotNull] InternalEntityEntry entry);
        Sidecar CreateStoreGeneratedValues([NotNull] InternalEntityEntry entry, [NotNull] IReadOnlyList<IProperty> properties);

        EntityKey CreateKey(
            [NotNull] IEntityType entityType,
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IPropertyAccessor propertyAccessor);
    }
}
