// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    internal static class MetadataHelper
    {
        public static void CheckSameEntityType(IReadOnlyList<Property> properties, string argumentName)
        {
            if (properties.Count > 1)
            {
                var entityType = properties[0].DeclaringEntityType;

                for (var i = 1; i < properties.Count; i++)
                {
                    if (properties[i].DeclaringEntityType != entityType ||
                        properties[i].DeclaringEntityType.FindProperty(properties[i].Name) != properties[i])
                    {
                        throw new ArgumentException(
                            CoreStrings.InconsistentEntityType(argumentName));
                    }
                }
            }
        }

        public static void CheckPropertiesInEntityType(IReadOnlyList<Property> properties, EntityType entityType, string argumentName)
        {
            foreach (var property in properties)
            {
                if (!property.DeclaringEntityType.IsAssignableFrom(entityType))
                {
                    throw new ArgumentException(
                            CoreStrings.InconsistentEntityType(argumentName));
                }
            }
        }
    }
}
