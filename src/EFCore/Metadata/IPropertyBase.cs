// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
        ///     This may be <c>null</c> for shadow properties or if mapped directly to a field.
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        ///     Gets the <see cref="FieldInfo" /> for the underlying CLR field for this property-like object.
        ///     This may be <c>null</c> for shadow properties or if the backing field is not known.
        /// </summary>
        FieldInfo FieldInfo { get; }
    }
}
