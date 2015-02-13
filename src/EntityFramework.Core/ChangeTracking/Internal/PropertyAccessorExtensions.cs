// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public static class PropertyAccessorExtensions
    {
        [NotNull]
        public static EntityKey GetDependentKeyValue([NotNull] this IPropertyAccessor propertyBagEntry, [NotNull] IForeignKey foreignKey)
            => propertyBagEntry.InternalEntityEntry.CreateKey(foreignKey.ReferencedEntityType, foreignKey.Properties, propertyBagEntry);

        [NotNull]
        public static EntityKey GetPrincipalKeyValue([NotNull] this IPropertyAccessor propertyBagEntry, [NotNull] IForeignKey foreignKey)
            => propertyBagEntry.InternalEntityEntry.CreateKey(foreignKey.ReferencedEntityType, foreignKey.ReferencedProperties, propertyBagEntry);

        [NotNull]
        public static EntityKey GetPrimaryKeyValue([NotNull] this IPropertyAccessor propertyBagEntry)
        {
            var entityType = propertyBagEntry.InternalEntityEntry.EntityType;
            return propertyBagEntry.InternalEntityEntry.CreateKey(entityType, entityType.GetPrimaryKey().Properties, propertyBagEntry);
        }
    }
}
