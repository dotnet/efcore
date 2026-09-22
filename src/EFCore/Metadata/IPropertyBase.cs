// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Base interface for navigations and properties.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IPropertyBase : IReadOnlyPropertyBase, IAnnotatable
{
    /// <summary>
    ///     Gets the type that this property-like object belongs to.
    /// </summary>
    new ITypeBase DeclaringType
    {
        [DebuggerStepThrough]
        get => (ITypeBase)((IReadOnlyPropertyBase)this).DeclaringType;
    }

    /// <summary>
    ///     Gets a <see cref="IClrPropertyGetter" /> for reading the value of this property.
    /// </summary>
    /// <remarks>
    ///     Note that it is an error to call this method for a shadow property (<see cref="IReadOnlyPropertyBase.IsShadowProperty" />)
    ///     since such a property has no associated <see cref="MemberInfo" />.
    /// </remarks>
    /// <returns>The accessor.</returns>
    IClrPropertyGetter GetGetter();

    /// <summary>
    ///     Gets the <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> that should be used to
    ///     get or set a value for the given property.
    /// </summary>
    /// <remarks>
    ///     Note that it is an error to call this method for a shadow property (<see cref="IReadOnlyPropertyBase.IsShadowProperty" />)
    ///     since such a property has no associated <see cref="MemberInfo" />.
    /// </remarks>
    /// <param name="forMaterialization">
    ///     If <see langword="true" />, then the member to use for query materialization will be returned.
    /// </param>
    /// <param name="forSet">
    ///     If <see langword="true" />, then the member to use for setting the property value will be returned, otherwise
    ///     the member to use for getting the property value will be returned.
    /// </param>
    /// <returns>The <see cref="MemberInfo" /> to use.</returns>
    MemberInfo GetMemberInfo(bool forMaterialization, bool forSet)
        => this.TryGetMemberInfo(forMaterialization, forSet, out var memberInfo, out var errorMessage)
            ? memberInfo!
            : throw new InvalidOperationException(errorMessage);

    /// <summary>
    ///     Gets the property index for this property.
    /// </summary>
    /// <returns>The index of the property.</returns>
    int GetIndex()
        => this.GetPropertyIndexes().Index;

    /// <summary>
    ///     Gets a <see cref="IComparer{T}" /> for comparing values in tracked <see cref="IUpdateEntry" /> entries.
    /// </summary>
    /// <returns>The comparer.</returns>
    IComparer<IUpdateEntry> GetCurrentValueComparer();
}
