// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Base interface for navigations and properties.
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
        ///         Note that it is an error to call this method for a shadow property (<see cref="IReadOnlyPropertyBase.IsShadowProperty" />)
        ///         since such a property has no associated <see cref="MemberInfo" />.
        ///     </para>
        /// </summary>
        /// <returns> The accessor. </returns>
        IClrPropertyGetter GetGetter();

        /// <summary>
        ///     <para>
        ///         Gets a <see cref="IComparer{T}" /> for comparing values in tracked <see cref="IUpdateEntry" /> entries.
        ///     </para>
        /// </summary>
        /// <returns> The comparer. </returns>
        IComparer<IUpdateEntry> GetCurrentValueComparer();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> that should be used to
        ///         get or set a value for the given property.
        ///     </para>
        ///     <para>
        ///         Note that it is an error to call this method for a shadow property (<see cref="IReadOnlyPropertyBase.IsShadowProperty" />)
        ///         since such a property has no associated <see cref="MemberInfo" />.
        ///     </para>
        /// </summary>
        /// <param name="forMaterialization">
        ///     If <see langword="true" />, then the member to use for query materialization will be returned.
        /// </param>
        /// <param name="forSet">
        ///     If <see langword="true" />, then the member to use for setting the property value will be returned, otherwise
        ///     the member to use for getting the property value will be returned.
        /// </param>
        /// <returns> The <see cref="MemberInfo" /> to use. </returns>
        MemberInfo GetMemberInfo(bool forMaterialization, bool forSet)
        {
            if (this.TryGetMemberInfo(forMaterialization, forSet, out var memberInfo, out var errorMessage))
            {
                return memberInfo!;
            }

            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        ///     Gets the property index for this property.
        /// </summary>
        /// <returns> The index of the property. </returns>
        int GetIndex()
            => this.GetPropertyIndexes().Index;
    }
}
