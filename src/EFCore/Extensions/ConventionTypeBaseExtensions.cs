// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionTypeBase" />.
    /// </summary>
    public static class ConventionTypeBaseExtensions
    {
        /// <summary>
        ///     Indicates whether the given member name is ignored.
        /// </summary>
        /// <param name="entityType"> The type to check the ignored member. </param>
        /// <param name="memberName"> The name of the member that might be ignored. </param>
        /// <returns> <c>true</c> if the given member name is ignored. </returns>
        public static bool IsIgnored([NotNull] this IConventionTypeBase entityType, [NotNull] string memberName)
            => entityType.FindIgnoredConfigurationSource(memberName) != null;

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for properties of this type.
        ///     </para>
        ///     <para>
        ///         Note that individual properties and navigations can override this access mode. The value set here will
        ///         be used for any property or navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The type to set the access mode for. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <c>null</c> to clear the mode set.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetPropertyAccessMode(
            [NotNull] this IConventionTypeBase entityType,
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AsTypeBase()
                .SetPropertyAccessMode(
                    propertyAccessMode,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="TypeBaseExtensions.GetPropertyAccessMode" />.
        /// </summary>
        /// <param name="entityType"> The type to set the access mode for. </param>
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
        /// <param name="entityType"> The type for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <c>null</c> to clear the mode set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetNavigationAccessMode(
            [NotNull] this IConventionTypeBase entityType,
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false)
            => Check.NotNull(entityType, nameof(entityType)).AsTypeBase()
                .SetNavigationAccessMode(
                    propertyAccessMode,
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="TypeBaseExtensions.GetNavigationAccessMode" />.
        /// </summary>
        /// <param name="entityType"> The type to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="TypeBaseExtensions.GetNavigationAccessMode" />. </returns>
        public static ConfigurationSource? GetNavigationAccessModeConfigurationSource([NotNull] this IConventionTypeBase entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.NavigationAccessMode)?.GetConfigurationSource();
    }
}
