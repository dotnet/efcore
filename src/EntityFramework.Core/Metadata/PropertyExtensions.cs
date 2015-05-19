// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class PropertyExtensions
    {
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

        public static void SetShadowIndex([NotNull] this Property property, int index)
        {
            Check.NotNull(property, nameof(property));

            if (index < 0
                || !property.IsShadowProperty)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            property[CoreAnnotationNames.ShadowIndexAnnotation] = index;
        }

        public static int GetOriginalValueIndex([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));
            Debug.Assert(property[CoreAnnotationNames.OriginalValueIndexAnnotation] != null);

            return (int)property[CoreAnnotationNames.OriginalValueIndexAnnotation];
        }

        public static void SetOriginalValueIndex([NotNull] this Property property, int index)
        {
            Check.NotNull(property, nameof(property));

            if (index < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            property[CoreAnnotationNames.OriginalValueIndexAnnotation] = index;
        }

        public static int? GetMaxLength([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.MaxLengthAnnotation];
        }

        public static void SetMaxLength([NotNull] this Property property, int? maxLength)
        {
            Check.NotNull(property, nameof(property));

            if (maxLength != null
                && maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            property[CoreAnnotationNames.MaxLengthAnnotation] = maxLength;
        }

        public static bool IsForeignKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: Avoid doing Contains check everywhere we need to know if a property is part of a foreign key
            return property.EntityType.GetForeignKeys().SelectMany(k => k.Properties).Contains(property);
        }

        public static bool IsPrimaryKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: make it fast to check if a property is part of the primary key
            return property.EntityType.GetPrimaryKey().Properties.Contains(property);
        }

        public static bool IsKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: make it fast to check if a property is part of a key
            return property.EntityType.GetKeys().SelectMany(e => e.Properties).Contains(property);
        }

        public static bool IsSentinelValue([NotNull] this IProperty property, [CanBeNull] object value)
        {
            Check.NotNull(property, nameof(property));

            return value == null || value.Equals(property.SentinelValue);
        }

        public static IProperty GetGenerationProperty([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var traversalList = new List<IProperty> { property };

            var index = 0;
            while (index < traversalList.Count)
            {
                var currentProperty = traversalList[index];

                if (currentProperty.IsValueGeneratedOnAdd)
                {
                    return currentProperty;
                }

                foreach (var foreignKey in currentProperty.EntityType.GetForeignKeys())
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
    }
}
