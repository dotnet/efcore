// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class PropertyBaseExtensions
    {
        public static int GetStoreGeneratedIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().StoreGenerationIndex;

        public static int GetRelationshipIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().RelationshipIndex;

        public static PropertyIndexes GetPropertyIndexes([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().PropertyIndexes;

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
                    property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

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

        private static void TrySetIndexes(IPropertyBase propertyBase, PropertyIndexes indexes)
            => propertyBase.AsPropertyBase().PropertyIndexes = indexes;

        public static PropertyAccessors GetPropertyAccessors([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Accessors;

        public static IClrPropertyGetter GetGetter([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Getter;

        public static IClrPropertySetter GetSetter([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Setter;

        public static PropertyInfo GetPropertyInfo([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().PropertyInfo;

        public static PropertyBase AsPropertyBase([NotNull] this IPropertyBase propertyBase, [NotNull] [CallerMemberName] string methodName = "")
            => propertyBase.AsConcreteMetadataType<IPropertyBase, PropertyBase>(methodName);
    }
}
