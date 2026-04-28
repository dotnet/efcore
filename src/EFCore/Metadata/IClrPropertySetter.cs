// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    /// <summary>
    ///     Sets the value of the property directly on the entity or closest complex collection element.
    /// </summary>
    /// <param name="instance">The entity or complex object instance.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The instance with the value set.</returns>
    object SetClrValue(object instance, object? value);

    /// <summary>
    ///     Sets the value of the property by reading it from <paramref name="reader" /> without boxing.
    /// </summary>
    /// <remarks>
    ///     The concrete implementation (<see cref="Internal.ClrPropertySetter{TEntity,TStructural,TValue}" />)
    ///     knows <c>TValue</c> at compile time and calls <c>reader.Read&lt;TValue&gt;(state)</c> directly,
    ///     eliminating both the per-type dispatch switch and boxing on the hot path.
    ///     The default implementation falls back to boxing via <see cref="SetClrValueUsingContainingEntity(object,object?)" />.
    /// </remarks>
    /// <param name="entity">The entity instance.</param>
    /// <param name="reader">The typed value reader for the current column.</param>
    /// <param name="state">The data-source state passed through to <paramref name="reader" />.</param>
    void SetClrValue<TState>(object entity, ITypedValueReader<TState> reader, TState state)
        => SetClrValueUsingContainingEntity(entity, reader.Read<object>(state));
}
