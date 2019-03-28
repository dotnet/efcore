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
    ///     Extension methods for <see cref="ITypeBase" />.
    /// </summary>
    public static class TypeBaseExtensions
    {
        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for properties and navigations of this type.
        ///     </para>
        ///     <para>
        ///         Note that individual properties and navigations can override this access mode. The value returned here will
        ///         be used for any property or navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="typeBase"> The type for which to get the access mode. </param>
        /// <returns> The access mode being used, or null if the default access mode is being used. </returns>
        public static PropertyAccessMode GetPropertyAccessMode(
            [NotNull] this ITypeBase typeBase)
            => (PropertyAccessMode?)Check.NotNull(typeBase, nameof(typeBase))[CoreAnnotationNames.PropertyAccessMode]
               ?? typeBase.Model.GetPropertyAccessMode();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for navigations of this type.
        ///     </para>
        ///     <para>
        ///         Note that individual navigations can override this access mode. The value returned here will
        ///         be used for any navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="typeBase"> The type for which to get the access mode. </param>
        /// <returns> The access mode being used, or null if the default access mode is being used. </returns>
        public static PropertyAccessMode GetNavigationAccessMode(
            [NotNull] this ITypeBase typeBase)
            => (PropertyAccessMode?)Check.NotNull(typeBase, nameof(typeBase))[CoreAnnotationNames.NavigationAccessMode]
               ?? typeBase.GetPropertyAccessMode();
    }
}
