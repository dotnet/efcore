// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="INavigation" />.
    /// </summary>
    public static class NavigationExtensions
    {
        /// <summary>
        ///     Gets a value indicating whether the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity.
        /// </summary>
        /// <param name="navigation"> The navigation property to check. </param>
        /// <returns>
        ///     True if the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity, otherwise false.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsDependentToPrincipal([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).ForeignKey.DependentToPrincipal == navigation;

        /// <summary>
        ///     Gets a value indicating whether the given navigation property is a collection property.
        /// </summary>
        /// <param name="navigation"> The navigation property to check. </param>
        /// <returns>
        ///     True if this is a collection property, false if it is a reference property.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsCollection([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            return !navigation.IsDependentToPrincipal() && !navigation.ForeignKey.IsUnique;
        }

        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns null if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation"> The navigation property to find the inverse of. </param>
        /// <returns>
        ///     The inverse navigation, or null if none is defined.
        /// </returns>
        [DebuggerStepThrough]
        public static INavigation FindInverse([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            return navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.PrincipalToDependent
                : navigation.ForeignKey.DependentToPrincipal;
        }

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation"> The navigation property to find the target entity type of. </param>
        /// <returns> The target entity type. </returns>
        [DebuggerStepThrough]
        public static IEntityType GetTargetType([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            return navigation.IsDependentToPrincipal()
                ? navigation.ForeignKey.PrincipalEntityType
                : navigation.ForeignKey.DeclaringEntityType;
        }
    }
}
