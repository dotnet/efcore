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
    ///     Extension methods for <see cref="IMutableTypeBase" />.
    /// </summary>
    public static class MutableTypeBaseExtensions
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
        /// <param name="entityType"> The type for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
        public static void SetPropertyAccessMode(
            [NotNull] this IMutableTypeBase entityType,
            PropertyAccessMode? propertyAccessMode)
            => Check.NotNull(entityType, nameof(entityType)).AsTypeBase()
                .SetPropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

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
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
        public static void SetNavigationAccessMode(
            [NotNull] this IMutableTypeBase entityType,
            PropertyAccessMode? propertyAccessMode)
            => Check.NotNull(entityType, nameof(entityType)).AsTypeBase()
                .SetNavigationAccessMode(propertyAccessMode, ConfigurationSource.Explicit);
    }
}
