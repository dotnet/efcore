// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IPropertyBase" />.
    /// </summary>
    public static class PropertyBaseExtensions
    {
        /// <summary>
        ///     Gets the name of the backing field for this property, or <c>null</c> if the backing field
        ///     is not known.
        /// </summary>
        /// <param name="propertyBase"> The property for which the backing field will be returned. </param>
        /// <returns> The name of the backing field, or <c>null</c>. </returns>
        public static string GetFieldName([NotNull] this IPropertyBase propertyBase)
            => propertyBase.FieldInfo?.GetSimpleMemberName();

        /// <summary>
        ///     Gets a value indicating whether this is a shadow property. A shadow property is one that does not have a
        ///     corresponding property in the entity class. The current value for the property is stored in
        ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>True</c> if the property is a shadow property, otherwise <c>false</c>.
        /// </returns>
        public static bool IsShadowProperty([NotNull] this IPropertyBase property)
            => Check.NotNull(property, nameof(property)).GetIdentifyingMemberInfo() == null;

        /// <summary>
        ///     Gets a value indicating whether this is an indexed property. An indexed property is one that does not have a
        ///     corresponding property in the entity class, rather the entity class has an indexer which takes the name
        ///     of the property as argument and returns an object.
        /// </summary>
        /// <param name="property"> The property to check. </param>
        /// <returns>
        ///     <c>True</c> if the property is an indexed property, otherwise <c>false</c>.
        /// </returns>
        public static bool IsIndexedProperty([NotNull] this IPropertyBase property)
        {
            Check.NotNull(property, nameof(property));

            var propertyInfo = property.PropertyInfo;
            return propertyInfo != null
                   && propertyInfo.IsEFIndexerProperty();
        }

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for this property.
        ///         <c>null</c> indicates that the default property access mode is being used.
        ///     </para>
        /// </summary>
        /// <param name="propertyBase"> The property for which to get the access mode. </param>
        /// <returns> The access mode being used, or <c>null</c> if the default access mode is being used. </returns>
        public static PropertyAccessMode GetPropertyAccessMode(
            [NotNull] this IPropertyBase propertyBase)
            => (PropertyAccessMode)(Check.NotNull(propertyBase, nameof(propertyBase))[CoreAnnotationNames.PropertyAccessMode]
                                    ?? (propertyBase is INavigation
                                        ? propertyBase.DeclaringType.GetNavigationAccessMode()
                                        : propertyBase.DeclaringType.GetPropertyAccessMode()));
    }
}
