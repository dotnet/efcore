// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public static class PropertyAccessorExtensions
    {
        public static EntityKey GetDependentKeyValue([NotNull] this IPropertyAccessor propertyAccessor, [NotNull] IForeignKey foreignKey)
            => propertyAccessor.InternalEntityEntry.CreateKey(
                foreignKey.PrincipalKey, foreignKey.Properties, propertyAccessor);

        public static EntityKey GetPrincipalKeyValue([NotNull] this IPropertyAccessor propertyAccessor, [NotNull] IForeignKey foreignKey)
            => propertyAccessor.GetPrincipalKeyValue(foreignKey.PrincipalKey);

        public static EntityKey GetPrincipalKeyValue([NotNull] this IPropertyAccessor propertyAccessor, [NotNull] IKey key)
            => propertyAccessor.InternalEntityEntry.CreateKey(
                key, key.Properties, propertyAccessor);

        public static EntityKey GetPrimaryKeyValue([NotNull] this IPropertyAccessor propertyAccessor)
        {
            var key = propertyAccessor.InternalEntityEntry.EntityType.GetPrimaryKey();
            return propertyAccessor.InternalEntityEntry.CreateKey(key, key.Properties, propertyAccessor);
        }
    }
}
