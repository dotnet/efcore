// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool ForAdd(this ValueGenerated valueGenerated)
            => (valueGenerated & ValueGenerated.OnAdd) != 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool ForUpdate(this ValueGenerated valueGenerated)
            => (valueGenerated & ValueGenerated.OnUpdate) != 0;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IEntityType> GetContainingEntityTypes([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetDerivedTypesInclusive();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IProperty property)
            => property.GetContainingKeys().SelectMany(k => k.GetReferencingForeignKeys());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IProperty GetGenerationProperty([NotNull] this IProperty property)
        {
            var traversalList = new List<IProperty> { property };

            var index = 0;
            while (index < traversalList.Count)
            {
                var currentProperty = traversalList[index];

                if (currentProperty.RequiresValueGenerator())
                {
                    return currentProperty;
                }

                foreach (var foreignKey in currentProperty.GetContainingForeignKeys())
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool RequiresValueGenerator([NotNull] this IProperty property)
            => (property.ValueGenerated.ForAdd()
                    && property.IsKey()
                    && (!property.IsForeignKey() || property.IsForeignKeyToSelf()))
                || property.GetValueGeneratorFactory() != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsForeignKeyToSelf([NotNull] this IProperty property)
        {
            Check.DebugAssert(property.IsKey(), "Only call this method for properties known to be part of a key.");

            foreach (var foreignKey in property.GetContainingForeignKeys())
            {
                var propertyIndex = foreignKey.Properties.IndexOf(property);
                if (propertyIndex == foreignKey.PrincipalKey.Properties.IndexOf(property))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool MayBeStoreGenerated([NotNull] this IProperty property)
        {
            if (property.ValueGenerated != ValueGenerated.Never)
            {
                return true;
            }

            if (property.IsKey()
                || property.IsForeignKey())
            {
                var generationProperty = property.GetGenerationProperty();
                return (generationProperty != null)
                    && (generationProperty.ValueGenerated != ValueGenerated.Never);
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool RequiresOriginalValue([NotNull] this IProperty property)
            => property.DeclaringEntityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.ChangingAndChangedNotifications
                || property.IsConcurrencyToken
                || property.IsKey()
                || property.IsForeignKey()
                || property.IsUniqueIndex();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Property AsProperty([NotNull] this IProperty property, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IProperty, Property>(property, methodName);
    }
}
