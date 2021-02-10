// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Base type for navigations and properties.
    /// </summary>
    public interface IPropertyBase : IReadOnlyPropertyBase, IAnnotatable
    {
        /// <summary>
        ///     Gets the type that this property-like object belongs to.
        /// </summary>
        new ITypeBase DeclaringType
        {
            [DebuggerStepThrough]
            get => (ITypeBase) ((IReadOnlyPropertyBase)this).DeclaringType;
        }

        /// <summary>
        ///     <para>
        ///         Gets a <see cref="IClrPropertyGetter" /> for reading the value of this property.
        ///     </para>
        ///     <para>
        ///         Note that it is an error to call this method for a shadow property (<see cref="PropertyBaseExtensions.IsShadowProperty" />)
        ///         since such a property has no associated <see cref="MemberInfo" />.
        ///     </para>
        /// </summary>
        /// <returns> The accessor. </returns>
        IClrPropertyGetter GetGetter();
    }
}
