// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     <para>
///         Represents operations backed by compiled delegates that support setting the value
///         of a mapped EF property.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IClrPropertySetter
{
    /// <summary>
    ///     Sets the value of the property using the containing entity instance.
    /// </summary>
    /// <param name="instance">The entity instance.</param>
    /// <param name="value">The value to set.</param>
    void SetClrValueUsingContainingEntity(object instance, object? value)
        => SetClrValueUsingContainingEntity(instance, [], value);

    /// <summary>
    ///     Sets the value of the property using the containing entity instance.
    /// </summary>
    /// <param name="instance">The entity instance.</param>
    /// <param name="indices"> The indices corresponding to complex collections used to access the property. </param>
    /// <param name="value">The value to set.</param>
    void SetClrValueUsingContainingEntity(object instance, IReadOnlyList<int> indices, object? value);
}
