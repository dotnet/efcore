// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableNavigation" />.
    /// </summary>
    public static class MutableNavigationExtensions
    {
        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns <see langword="null" /> if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation"> The navigation property to find the inverse of. </param>
        /// <returns>
        ///     The inverse navigation, or <see langword="null" /> if none is defined.
        /// </returns>
        [Obsolete("Use IMutableNavigation.Inverse")]
        public static IMutableNavigation FindInverse([NotNull] this IMutableNavigation navigation)
            => navigation.Inverse;

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation"> The navigation property to find the target entity type of. </param>
        /// <returns> The target entity type. </returns>
        [Obsolete("Use IMutableNavigation.TargetEntityType")]
        public static IMutableEntityType GetTargetType([NotNull] this IMutableNavigation navigation)
            => navigation.TargetEntityType;
    }
}
