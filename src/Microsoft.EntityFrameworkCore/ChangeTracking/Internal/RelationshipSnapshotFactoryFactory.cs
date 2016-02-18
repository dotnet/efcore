// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class RelationshipSnapshotFactoryFactory : SnapshotFactoryFactory<IUpdateEntry>
    {
        protected override int GetPropertyIndex(IPropertyBase propertyBase)
            => propertyBase.GetRelationshipIndex();

        protected override int GetPropertyCount(IEntityType entityType)
            => entityType.RelationshipPropertyCount();
    }
}
