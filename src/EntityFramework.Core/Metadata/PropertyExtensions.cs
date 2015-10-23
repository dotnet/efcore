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
                || !((IProperty)property).IsShadowProperty)
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

        public static void SetMaxLength([NotNull] this IMutableProperty property, int? maxLength)
        {
            Check.NotNull(property, nameof(property));

            if (maxLength != null
                && maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            property[CoreAnnotationNames.MaxLengthAnnotation] = maxLength;
        }

        public static bool IsForeignKey([NotNull] this IProperty property, [NotNull] IEntityType entityType)
            => FindContainingForeignKeys(property, entityType).Any();

        public static bool IsPrimaryKey([NotNull] this IProperty property)
            => FindContainingPrimaryKey(property) != null;

        public static bool IsKey([NotNull] this IProperty property)
            => FindContainingKeys(property).Any();

        public static IEnumerable<IEntityType> FindContainingEntityTypes([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: Avoid doing Contains check each time
            return property.DeclaringEntityType.Model.GetEntityTypes().Where(e => e.GetProperties().Contains(property));
        }

        public static IEnumerable<EntityType> FindContainingEntityTypes([NotNull] this Property property)
        {
            Check.NotNull(property, nameof(property));
            
            return ((IProperty)property).FindContainingEntityTypes().Cast<EntityType>();
        }

        public static IEnumerable<IForeignKey> FindContainingForeignKeys([NotNull] this IProperty property, [NotNull] IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(property, nameof(entityType));

            // TODO: Also search for FKs on derived types
            // Issue #2514

            return entityType.GetForeignKeys().Where(k => k.Properties.Contains(property));
        }

        public static IEnumerable<IForeignKey> FindContainingForeignKeysInHierarchy([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var entityType = property.DeclaringEntityType;
            return new[] { entityType }.Concat(entityType.GetDerivedTypes())
                .SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(k => k.Properties.Contains(property));
        }

        public static IEnumerable<ForeignKey> FindContainingForeignKeys([NotNull] this Property property, [NotNull] EntityType entityType)
            => ((IProperty)property).FindContainingForeignKeys(entityType).Cast<ForeignKey>();

        public static IEnumerable<ForeignKey> FindContainingForeignKeysInHierarchy([NotNull] this Property property)
            => ((IProperty)property).FindContainingForeignKeysInHierarchy().Cast<ForeignKey>();

        public static IKey FindContainingPrimaryKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: make it fast to check if a property is part of the primary key
            var pk = property.DeclaringEntityType.GetPrimaryKey();
            if (pk != null
                && pk.Properties.Contains(property))
            {
                return pk;
            }

            return null;
        }

        public static Key FindContainingPrimaryKey([NotNull] this Property property)
            => (Key)((IProperty)property).FindContainingPrimaryKey();

        public static IEnumerable<IKey> FindContainingKeys([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: make it fast to check if a property is part of a key
            return property.DeclaringEntityType.GetKeys().Where(e => e.Properties.Contains(property));
        }
        
        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IProperty property)
            => property.DeclaringEntityType.Model.FindReferencingForeignKeys(property);
        
        public static IEnumerable<ForeignKey> FindReferencingForeignKeys([NotNull] this Property property)
            => property.DeclaringEntityType.Model.FindReferencingForeignKeys(property);

        public static IEnumerable<Key> FindContainingKeys([NotNull] this Property property)
            => ((IProperty)property).FindContainingKeys().Cast<Key>();

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
    }
}
