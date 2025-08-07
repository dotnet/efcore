// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     <para>
///         Represents operations backed by compiled delegates that support getting the value
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
public interface IClrPropertyGetter
{
    /// <summary>
    ///     Gets the property value from the containing entity instance.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <returns>The property value.</returns>
    object? GetClrValueUsingContainingEntity(object entity)
        => GetClrValueUsingContainingEntity(entity, []);

    /// <summary>
    ///     Gets the property value from the containing entity instance.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="indices"> The indices corresponding to complex collections used to access the property. </param>
    /// <returns>The property value.</returns>
    object? GetClrValueUsingContainingEntity(object entity, IReadOnlyList<int> indices);

    /// <summary>
    ///     Checks whether or not the property from the containing entity instance is set to the CLR default for its type.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <returns><see langword="true" /> if the property value is the CLR default; <see langword="false" /> it is any other value.</returns>
    bool HasSentinelValueUsingContainingEntity(object entity)
        => HasSentinelValueUsingContainingEntity(entity, []);

    /// <summary>
    ///     Checks whether or not the property from the containing entity instance is set to the CLR default for its type.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <param name="indices"> The indices corresponding to complex collections used to access the property. </param>
    /// <returns><see langword="true" /> if the property value is the CLR default; <see langword="false" /> it is any other value.</returns>
    bool HasSentinelValueUsingContainingEntity(object entity, IReadOnlyList<int> indices);

    /// <summary>
    ///     Gets the property value from the declaring type.
    /// </summary>
    /// <param name="structuralObject">The entity or complex type instance.</param>
    /// <returns>The property value.</returns>
    object? GetClrValue(object structuralObject);

    /// <summary>
    ///     Checks whether or not the property is set to the CLR default for its type.
    /// </summary>
    /// <param name="structuralObject">The entity or complex type instance.</param>
    /// <returns><see langword="true" /> if the property value is the CLR default; <see langword="false" /> it is any other value.</returns>
    bool HasSentinelValue(object structuralObject);
}
