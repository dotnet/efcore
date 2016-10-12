// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="ITypeBase" />.
    /// </summary>
    public static class TypeBaseExtensions
    {
        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for properties of this type.
        ///         Null indicates that the default property access mode is being used.
        ///     </para>
        ///     <para>
        ///         Note that individual properties can override this access mode. The value returned here will
        ///         be used for any property for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="typeBase"> The type for which to get the access mode. </param>
        /// <returns> The access mode being used, or null if the default access mode is being used. </returns>
        public static PropertyAccessMode? GetPropertyAccessMode(
            [NotNull] this ITypeBase typeBase)
            => (PropertyAccessMode?)Check.NotNull(typeBase, nameof(typeBase))[CoreAnnotationNames.PropertyAccessModeAnnotation]
               ?? typeBase.Model.GetPropertyAccessMode();
    }
}
