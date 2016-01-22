// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class PropertyExtensions
    {
        public static IEnumerable<IEntityType> FindContainingEntityTypes([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.DeclaringEntityType.GetDerivedTypesInclusive();
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

        public static int GetStoreGeneratedIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().StoreGenerationIndex;

        public static int GetRelationshipIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().RelationshipIndex;

        public static int GetShadowIndex([NotNull] this IProperty property)
            => property.GetPropertyIndexes().ShadowIndex;

        public static int GetOriginalValueIndex([NotNull] this IProperty property)
            => property.GetPropertyIndexes().OriginalValueIndex;

        public static int GetIndex([NotNull] this IProperty property)
            => property.GetPropertyIndexes().Index;

        private static PropertyIndexes GetPropertyIndexes(this IPropertyBase propertyBase)
        {
            var indexesAccessor = propertyBase as IPropertyIndexesAccessor;

            return indexesAccessor != null
                ? indexesAccessor.Indexes
                : propertyBase.DeclaringEntityType.CalculateIndexes(propertyBase);
        }

        public static PropertyIndexes CalculateIndexes([NotNull] this IEntityType entityType, [NotNull] IPropertyBase propertyBase)
        {
            var index = 0;
            var shadowIndex = 0;
            var originalValueIndex = 0;
            var relationshipIndex = 0;
            var storeGenerationIndex = 0;

            var baseCounts = entityType.BaseType?.GetCounts();
            if (baseCounts != null)
            {
                index = baseCounts.PropertyCount;
                shadowIndex = baseCounts.ShadowCount;
                originalValueIndex = baseCounts.OriginalValueCount;
                relationshipIndex = baseCounts.RelationshipCount;
                storeGenerationIndex = baseCounts.StoreGeneratedCount;
            }

            PropertyIndexes callingPropertyIndexes = null;

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var indexes = new PropertyIndexes(
                    index++,
                    property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                    property.IsShadowProperty ? shadowIndex++ : -1,
                    property.IsKeyOrForeignKey() ? relationshipIndex++ : -1,
                    MayBeStoreGenerated(property) ? storeGenerationIndex++ : -1);

                TrySetIndexes(property, indexes);

                if (propertyBase == property)
                {
                    callingPropertyIndexes = indexes;
                }
            }

            foreach (var navigation in entityType.GetDeclaredNavigations())
            {
                var indexes = new PropertyIndexes(index++, -1, -1, relationshipIndex++, -1);

                TrySetIndexes(navigation, indexes);

                if (propertyBase == navigation)
                {
                    callingPropertyIndexes = indexes;
                }
            }

            foreach (var derivedType in entityType.GetDirectlyDerivedTypes())
            {
                derivedType.CalculateIndexes(propertyBase);
            }

            return callingPropertyIndexes;
        }

        public static bool MayBeStoreGenerated([NotNull] this IProperty property)
        {
            if (property.ValueGenerated != ValueGenerated.Never)
            {
                return true;
            }

            if (property.IsKeyOrForeignKey())
            {
                var generationProperty = property.GetGenerationProperty();
                return (generationProperty != null)
                       && (generationProperty.ValueGenerated != ValueGenerated.Never);
            }

            return false;
        }

        private static void TrySetIndexes(IPropertyBase propertyBase, PropertyIndexes indexes)
        {
            var indexAccessor = propertyBase as IPropertyIndexesAccessor;
            if (indexAccessor != null)
            {
                indexAccessor.Indexes = indexes;
            }
        }

        public static bool RequiresOriginalValue([NotNull] this IProperty property)
            => property.DeclaringEntityType.UseEagerSnapshots()
               || property.IsConcurrencyToken
               || property.IsForeignKey();

        public static bool IsKeyOrForeignKey([NotNull] this IProperty property)
            => property.IsKey()
               || property.IsForeignKey();
    }
}
