// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class PropertyBaseExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetStoreGeneratedIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().StoreGenerationIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetRelationshipIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().RelationshipIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetIndex([NotNull] this IPropertyBase property)
            => property.GetPropertyIndexes().Index;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyIndexes GetPropertyIndexes([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().PropertyIndexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                    index: index++,
                    originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                    shadowIndex: property.IsShadowProperty ? shadowIndex++ : -1,
                    relationshipIndex: property.IsKeyOrForeignKey() ? relationshipIndex++ : -1,
                    storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

                TrySetIndexes(property, indexes);

                if (propertyBase == property)
                {
                    callingPropertyIndexes = indexes;
                }
            }

            var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

            foreach (var navigation in entityType.GetDeclaredNavigations())
            {
                var indexes = new PropertyIndexes(
                    index: index++,
                    originalValueIndex: -1,
                    shadowIndex: -1,
                    relationshipIndex: navigation.IsCollection() && isNotifying ? -1 : relationshipIndex++,
                    storeGenerationIndex: -1);

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

        private static void TrySetIndexes(IPropertyBase propertyBase, PropertyIndexes indexes)
            => propertyBase.AsPropertyBase().PropertyIndexes = indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyAccessors GetPropertyAccessors([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Accessors;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IClrPropertyGetter GetGetter([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Getter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IClrPropertySetter GetSetter([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Setter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyInfo GetPropertyInfo([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().PropertyInfo;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyBase AsPropertyBase([NotNull] this IPropertyBase propertyBase, [NotNull] [CallerMemberName] string methodName = "")
            => propertyBase.AsConcreteMetadataType<IPropertyBase, PropertyBase>(methodName);
    }
}
