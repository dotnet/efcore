// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionNavigation" />.
    /// </summary>
    [Obsolete("Use IConventionNavigation")]
    public static class ConventionNavigationExtensions
    {
        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns null if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation"> The navigation property to find the inverse of. </param>
        /// <returns>
        ///     The inverse navigation, or <see langword="null" /> if none is defined.
        /// </returns>
        [Obsolete("Use IConventionNavigation.Inverse")]
        public static IConventionNavigation? FindInverse(this IConventionNavigation navigation)
            => navigation.Inverse;

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation"> The navigation property to find the target entity type of. </param>
        /// <returns> The target entity type. </returns>
        [Obsolete("Use IConventionNavigation.TargetEntityType")]
        public static IConventionEntityType GetTargetType(this IConventionNavigation navigation)
            => navigation.TargetEntityType;
    }
}
