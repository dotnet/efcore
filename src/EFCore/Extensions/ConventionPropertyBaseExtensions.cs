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
    ///     Extension methods for <see cref="IConventionPropertyBase" />.
    /// </summary>
    public static class ConventionPropertyBaseExtensions
    {
        /// <summary>
        ///     <para>
        ///         Sets the underlying CLR field that this property should use.
        ///         This may be <c>null</c> for shadow properties or if the backing field for the property is not known.
        ///     </para>
        ///     <para>
        ///         Backing fields are normally found by convention as described
        ///         here: http://go.microsoft.com/fwlink/?LinkId=723277.
        ///         This method is useful for setting backing fields explicitly in cases where the
        ///         correct field is not found by convention.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. This can be changed by calling
        ///         <see cref="SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property for which the backing field should be set. </param>
        /// <param name="fieldName"> The name of the field to use. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetField(
            [NotNull] this IConventionPropertyBase property,
            [CanBeNull] string fieldName,
            bool fromDataAnnotation = false)
            => Check.NotNull(property, nameof(property)).AsPropertyBase()
                .SetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
        /// </summary>
        /// <param name="property"> The property for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or null to clear the mode set.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetPropertyAccessMode(
            [NotNull] this IConventionPropertyBase property,
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false)
            => Check.NotNull(property, nameof(property)).AsPropertyBase()
                .SetPropertyAccessMode(
                    propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="IPropertyBase.GetPropertyAccessMode" />.
        /// </summary>
        /// <param name="property"> The property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="IPropertyBase.GetPropertyAccessMode" />. </returns>
        public static ConfigurationSource? GetPropertyAccessModeConfigurationSource([NotNull] this IConventionPropertyBase property)
            => property.FindAnnotation(CoreAnnotationNames.PropertyAccessMode)?.GetConfigurationSource();
    }
}
