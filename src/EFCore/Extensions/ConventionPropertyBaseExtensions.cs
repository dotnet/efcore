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
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
        /// </summary>
        /// <param name="property"> The property for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or null to clear the mode set.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static PropertyAccessMode? SetPropertyAccessMode(
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
