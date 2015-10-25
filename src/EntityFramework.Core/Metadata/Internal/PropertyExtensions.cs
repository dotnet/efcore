// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class PropertyExtensions
    {
        public static IEnumerable<IEntityType> FindContainingEntityTypes([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.DeclaringEntityType.GetDerivedTypesInclusive();
        }

        public static IEnumerable<IForeignKey> FindContainingForeignKeys(
            [NotNull] this IProperty property, [NotNull] IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(property, nameof(entityType));

            return entityType.GetForeignKeys().Where(k => k.Properties.Contains(property));
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.FindContainingKeys().SelectMany(k => k.FindReferencingForeignKeys());
        }

        public static IProperty GetGenerationProperty([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var traversalList = new List<IProperty> { property };

            var index = 0;
            while (index < traversalList.Count)
            {
                var currentProperty = traversalList[index];

                if (currentProperty.RequiresValueGenerator)
                {
                    return currentProperty;
                }

                foreach (var foreignKey in currentProperty.DeclaringEntityType.GetForeignKeys())
                {
                    for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                    {
                        if (currentProperty == foreignKey.Properties[propertyIndex])
                        {
                            var nextProperty = foreignKey.PrincipalKey.Properties[propertyIndex];
                            if (!traversalList.Contains(nextProperty))
                            {
                                traversalList.Add(nextProperty);
                            }
                        }
                    }
                }
                index++;
            }
            return null;
        }

        public static int GetShadowIndex([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (!property.IsShadowProperty)
            {
                return -1;
            }

            if (property[CoreAnnotationNames.ShadowIndexAnnotation] == null)
            {
                return 0;
            }

            return (int)property[CoreAnnotationNames.ShadowIndexAnnotation];
        }

        public static int GetOriginalValueIndex([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));
            Debug.Assert(property[CoreAnnotationNames.OriginalValueIndexAnnotation] != null);

            return (int)property[CoreAnnotationNames.OriginalValueIndexAnnotation];
        }

        public static int GetIndex([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int)property[CoreAnnotationNames.IndexAnnotation];
        }

        public static bool IsForeignKey([NotNull] this IProperty property, [NotNull] IEntityType entityType)
            => FindContainingForeignKeys(property, entityType).Any();
    }
}
