// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionNavigation" />.
    /// </summary>
    public static class ConventionNavigationExtensions
    {
        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns null if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation"> The navigation property to find the inverse of. </param>
        /// <returns>
        ///     The inverse navigation, or <c>null</c> if none is defined.
        /// </returns>
        public static IConventionNavigation FindInverse([NotNull] this IConventionNavigation navigation)
            => (IConventionNavigation)((INavigation)navigation).FindInverse();

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation"> The navigation property to find the target entity type of. </param>
        /// <returns> The target entity type. </returns>
        public static IConventionEntityType GetTargetType([NotNull] this IConventionNavigation navigation)
            => (IConventionEntityType)((INavigation)navigation).GetTargetType();

        /// <summary>
        ///     Sets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        /// <param name="navigation"> The navigation property to set whether it should be eager loaded. </param>
        /// <param name="eagerLoaded"> A value indicating whether this navigation should be eager loaded by default. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetIsEagerLoaded(
            [NotNull] this IConventionNavigation navigation,
            bool? eagerLoaded,
            bool fromDataAnnotation = false)
            => navigation.AsNavigation().SetIsEagerLoaded(
                eagerLoaded, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Returns the configuration source for <see cref="NavigationExtensions.IsEagerLoaded" />.
        /// </summary>
        /// <param name="navigation"> The navigation property to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="NavigationExtensions.IsEagerLoaded" />. </returns>
        public static ConfigurationSource? GetIsEagerLoadedConfigurationSource([NotNull] this IConventionNavigation navigation)
            => navigation.FindAnnotation(CoreAnnotationNames.EagerLoaded)?.GetConfigurationSource();
    }
}
