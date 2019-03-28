// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Extensions
{
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
        /// <param name="entityType"> The entity type for which to set the access mode. </param>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <c>null</c> to clear the mode set.</param>
        public static void SetPropertyAccessMode(
            [NotNull] this IMutableTypeBase entityType, PropertyAccessMode? propertyAccessMode)
        {
            Check.NotNull(entityType, nameof(entityType));

            entityType[CoreAnnotationNames.PropertyAccessMode] = propertyAccessMode;
        }

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
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <c>null</c> to clear the mode set.</param>
        public static void SetNavigationAccessMode(
            [NotNull] this IMutableTypeBase entityType, PropertyAccessMode? propertyAccessMode)
        {
            Check.NotNull(entityType, nameof(entityType));

            entityType[CoreAnnotationNames.NavigationAccessMode] = propertyAccessMode;
        }
    }
}
