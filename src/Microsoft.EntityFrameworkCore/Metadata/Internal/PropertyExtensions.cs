// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IEntityType> GetContainingEntityTypes([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetDerivedTypesInclusive();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IProperty property)
            => property.GetContainingKeys().SelectMany(k => k.GetReferencingForeignKeys());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IProperty GetGenerationProperty([NotNull] this IProperty property)
        {
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetShadowIndex([NotNull] this IProperty property)
            => property.GetPropertyIndexes().ShadowIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetOriginalValueIndex([NotNull] this IProperty property)
            => property.GetPropertyIndexes().OriginalValueIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool RequiresOriginalValue([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.ChangingAndChangedNotifications
               || property.IsConcurrencyToken
               || property.IsForeignKey();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsKeyOrForeignKey([NotNull] this IProperty property)
            => property.IsKey()
               || property.IsForeignKey();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Property AsProperty([NotNull] this IProperty property, [NotNull] [CallerMemberName] string methodName = "")
            => property.AsConcreteMetadataType<IProperty, Property>(methodName);
    }
}
