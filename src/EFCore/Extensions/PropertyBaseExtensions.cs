// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IPropertyBase" />.
    /// </summary>
    public static class PropertyBaseExtensions
    {
        /// <summary>
        ///     Gets the name of the backing field for this property, or null if the backing field
        ///     is not known.
        /// </summary>
        /// <param name="propertyBase"> The property for which the backing field will be returned. </param>
        /// <returns> The name of the backing field, or null. </returns>
        public static string GetFieldName([NotNull] this IPropertyBase propertyBase)
            => propertyBase.FieldInfo?.GetSimpleMemberName();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for this property.
        ///         Null indicates that the default property access mode is being used.
        ///     </para>
        /// </summary>
        /// <param name="propertyBase"> The property for which to get the access mode. </param>
        /// <returns> The access mode being used, or null if the default access mode is being used. </returns>
        public static PropertyAccessMode? GetPropertyAccessMode(
            [NotNull] this IPropertyBase propertyBase)
            => (PropertyAccessMode?)Check.NotNull(propertyBase, nameof(propertyBase))[CoreAnnotationNames.PropertyAccessModeAnnotation]
               ?? (propertyBase is INavigation
                   ? propertyBase.DeclaringType.GetNavigationAccessMode()
                   : propertyBase.DeclaringType.GetPropertyAccessMode());
    }
}
