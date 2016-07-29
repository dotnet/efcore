// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableNavigation" />.
    /// </summary>
    public static class MutableNavigationExtensions
    {
        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns null if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation"> The navigation property to find the inverse of. </param>
        /// <returns>
        ///     The inverse navigation, or null if none is defined.
        /// </returns>
        public static IMutableNavigation FindInverse([NotNull] this IMutableNavigation navigation)
            => (IMutableNavigation)((INavigation)navigation).FindInverse();

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation"> The navigation property to find the target entity type of. </param>
        /// <returns> The target entity type. </returns>
        public static IMutableEntityType GetTargetType([NotNull] this IMutableNavigation navigation)
            => (IMutableEntityType)((INavigation)navigation).GetTargetType();

        /// <summary>
        ///     <para>
        ///         Sets the backing field to use for this navigation property.
        ///     </para>
        ///     <para>
        ///         Backing fields are normally found by convention as described
        ///         here: http://go.microsoft.com/fwlink/?LinkId=723277.
        ///         This method is useful for setting backing fields explicitly in cases where the
        ///         correct field is not found by convention.
        ///     </para>
        ///     <para>
        ///         By default, the backing field is only used for navigation properties when the property
        ///         must be read and there is no getter, or the property must be set and there is not setter.
        ///         This can be changed by calling <see cref="SetPropertyAccessMode" /> for the property.
        ///     </para>
        /// </summary>
        /// <param name="navigation"> The navigation property for which the field should be set. </param>
        /// <param name="fieldName"> The name of the field to use. </param>
        public static void SetField([NotNull] this IMutableNavigation navigation, [CanBeNull] string fieldName)
            => Check.NotNull(navigation, nameof(navigation)).AsPropertyBase()
                .SetField(fieldName, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this navigation property.
        /// </summary>
        /// <param name="navigation"> The navigation property for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or null to clear the mode set.</param>
        public static void SetPropertyAccessMode(
            [NotNull] this IMutableNavigation navigation, PropertyAccessMode? propertyAccessMode)
        {
            Check.NotNull(navigation, nameof(navigation));

            navigation[CoreAnnotationNames.PropertyAccessModeAnnotation] = propertyAccessMode;
        }
    }
}
