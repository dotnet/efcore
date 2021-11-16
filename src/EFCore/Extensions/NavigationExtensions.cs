// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IReadOnlyNavigation" />.
    /// </summary>
    [Obsolete("Use IReadOnlyNavigation")]
    public static class NavigationExtensions
    {
        /// <summary>
        ///     Gets a value indicating whether the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity.
        /// </summary>
        /// <param name="navigation">The navigation property to check.</param>
        /// <returns>
        ///     <see langword="true" /> if the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity, otherwise <see langword="false" />.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.IsOnDependent")]
        public static bool IsDependentToPrincipal(this INavigation navigation)
            => navigation.IsOnDependent;

        /// <summary>
        ///     Gets a value indicating whether the given navigation property is a collection property.
        /// </summary>
        /// <param name="navigation">The navigation property to check.</param>
        /// <returns>
        ///     <see langword="true" /> if this is a collection property, false if it is a reference property.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.IsCollection")]
        public static bool IsCollection(this INavigation navigation)
            => navigation.IsCollection;

        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns null if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation">The navigation property to find the inverse of.</param>
        /// <returns>
        ///     The inverse navigation, or <see langword="null" /> if none is defined.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.Inverse")]
        public static INavigation? FindInverse(this INavigation navigation)
            => navigation.Inverse;

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation">The navigation property to find the target entity type of.</param>
        /// <returns>The target entity type.</returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.TargetEntityType")]
        public static IEntityType GetTargetType(this INavigation navigation)
            => navigation.TargetEntityType;

        /// <summary>
        ///     Gets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        /// <param name="navigation">The navigation property to find whether it should be eager loaded.</param>
        /// <returns>A value indicating whether this navigation should be eager loaded by default.</returns>
        [Obsolete("Use IReadOnlyNavigation.IsEagerLoaded")]
        public static bool IsEagerLoaded(this INavigation navigation)
            => navigation.IsEagerLoaded;
    }
}
