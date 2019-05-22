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
        ///     Gets the name of the property.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        ITypeBase DeclaringType { get; }

        /// <summary>
        ///     Gets the type of value that this property holds.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo" /> for the underlying CLR property that this
        ///     object represents. This may be <c>null</c> for shadow properties or properties mapped directly to fields.
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        ///     Gets the <see cref="FieldInfo" /> for the underlying CLR field for this property.
        ///     This may be <c>null</c> for shadow properties or if the backing field for the property is not known.
        /// </summary>
        FieldInfo FieldInfo { get; }
    }
}
