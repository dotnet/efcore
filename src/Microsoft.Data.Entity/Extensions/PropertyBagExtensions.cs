// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public static class PropertyBagExtensions
    {
        [NotNull]
        public static EntityKey GetDependentKeyValue([NotNull] this IPropertyBagEntry propertyBagEntry, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(propertyBagEntry, "propertyBagEntry");
            Check.NotNull(foreignKey, "foreignKey");

            return propertyBagEntry.StateEntry.CreateKey(foreignKey.ReferencedEntityType, foreignKey.Properties, propertyBagEntry);
        }

        [NotNull]
        public static EntityKey GetPrincipalKeyValue([NotNull] this IPropertyBagEntry propertyBagEntry, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(propertyBagEntry, "propertyBagEntry");
            Check.NotNull(foreignKey, "foreignKey");

            var keyValue = propertyBagEntry.StateEntry.CreateKey(foreignKey.ReferencedEntityType, foreignKey.ReferencedProperties, propertyBagEntry);
            if (keyValue == EntityKey.NullEntityKey)
            {
                throw new InvalidOperationException(
                    Strings.FormatNullPrincipalKey(
                        propertyBagEntry.StateEntry.EntityType.Name,
                        string.Join(", ", foreignKey.ReferencedProperties.Select(p => p.Name))));
            }

            return keyValue;
        }
    }
}
