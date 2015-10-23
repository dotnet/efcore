// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public static class PropertyAccessorExtensions
    {
        public static IKeyValue GetDependentKeyValue([NotNull] this IPropertyAccessor propertyAccessor, [NotNull] IForeignKey foreignKey)
            => propertyAccessor.InternalEntityEntry.CreateKey(
                foreignKey.PrincipalKey, foreignKey.Properties, propertyAccessor);

        public static IKeyValue GetPrincipalKeyValue([NotNull] this IPropertyAccessor propertyAccessor, [NotNull] IForeignKey foreignKey)
            => propertyAccessor.GetPrincipalKeyValue(foreignKey.PrincipalKey);

        public static IKeyValue GetPrincipalKeyValue([NotNull] this IPropertyAccessor propertyAccessor, [NotNull] IKey key)
            => propertyAccessor.InternalEntityEntry.CreateKey(
                key, key.Properties, propertyAccessor);

        public static IKeyValue GetPrimaryKeyValue([NotNull] this IPropertyAccessor propertyAccessor)
        {
            var key = propertyAccessor.InternalEntityEntry.EntityType.FindPrimaryKey();
            return propertyAccessor.InternalEntityEntry.CreateKey(key, key.Properties, propertyAccessor);
        }
    }
}
