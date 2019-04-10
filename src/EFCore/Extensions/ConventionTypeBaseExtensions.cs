// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionTypeBase" />.
    /// </summary>
    public static class ConventionTypeBaseExtensions
    {
        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for properties and navigations of this entity type.
        ///     </para>
        ///     <para>
        ///         Note that individual properties and navigations can override this access mode. The value set here will
        ///         be used for any property or navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <c>null</c> to clear the mode set.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetPropertyAccessMode(
            [NotNull] this IConventionTypeBase entityType,
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityType, nameof(entityType));

            entityType.SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns the configuration source for <see cref="TypeBaseExtensions.GetPropertyAccessMode" />.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="TypeBaseExtensions.GetPropertyAccessMode" />. </returns>
        public static ConfigurationSource? GetPropertyAccessModeConfigurationSource([NotNull] this IConventionTypeBase entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.PropertyAccessMode)?.GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for navigations of this entity type.
        ///     </para>
        ///     <para>
        ///         Note that individual navigations can override this access mode. The value set here will
        ///         be used for any navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <c>null</c> to clear the mode set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetNavigationAccessMode(
            [NotNull] this IConventionTypeBase entityType,
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityType, nameof(entityType));

            entityType.SetOrRemoveAnnotation(CoreAnnotationNames.NavigationAccessMode, propertyAccessMode, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns the configuration source for <see cref="TypeBaseExtensions.GetNavigationAccessMode" />.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="TypeBaseExtensions.GetNavigationAccessMode" />. </returns>
        public static ConfigurationSource? GetNavigationAccessModeConfigurationSource([NotNull] this IConventionTypeBase entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.NavigationAccessMode)?.GetConfigurationSource();
    }
}
