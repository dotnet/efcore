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
    ///     Sets the value of the property.
    /// </summary>
    /// <param name="instance">The entity instance.</param>
    /// <param name="value">The value to set.</param>
    void SetClrValue(object instance, object? value);
}
