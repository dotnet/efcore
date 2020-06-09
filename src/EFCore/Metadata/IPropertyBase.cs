// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Base type for navigation and scalar properties.
    /// </summary>
    public interface IPropertyBase : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of this property-like object.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type that this property-like object belongs to.
        /// </summary>
        ITypeBase DeclaringType { get; }

        /// <summary>
        ///     Gets the type of value that this property-like object holds.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo" /> for the underlying CLR property for this property-like object.
        ///     This may be <see langword="null" /> for shadow properties or if mapped directly to a field.
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        ///     Gets the <see cref="FieldInfo" /> for the underlying CLR field for this property-like object.
        ///     This may be <see langword="null" /> for shadow properties or if the backing field is not known.
        /// </summary>
        FieldInfo FieldInfo { get; }

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for this property.
        ///         <see langword="null" /> indicates that the default property access mode is being used.
        ///     </para>
        /// </summary>
        /// <returns> The access mode being used, or <see langword="null" /> if the default access mode is being used. </returns>
        PropertyAccessMode GetPropertyAccessMode()
            => (PropertyAccessMode)(this[CoreAnnotationNames.PropertyAccessMode]
                ?? PropertyAccessMode.PreferField);
    }
}
