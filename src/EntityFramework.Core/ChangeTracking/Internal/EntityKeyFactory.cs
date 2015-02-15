// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class EntityKeyFactory
    {
        [NotNull]
        public abstract EntityKey Create(
            [NotNull] IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IValueReader valueReader);

        [NotNull]
        public abstract EntityKey Create(
            [NotNull] IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IPropertyAccessor propertyAccessor);
    }
}
