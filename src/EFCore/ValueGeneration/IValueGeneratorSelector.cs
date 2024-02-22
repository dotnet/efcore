// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     <para>
///         Selects value generators to be used to generate values for properties of entities.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IValueGeneratorSelector
{
    /// <summary>
    ///     Selects the appropriate value generator for a given property.
    /// </summary>
    /// <param name="property">The property to get the value generator for.</param>
    /// <param name="typeBase">
    ///     The type that the value generator will be used for. When called on inherited properties on derived types,
    ///     this type may be different from the declaring type for <paramref name="property" />
    /// </param>
    /// <returns>The value generator to be used.</returns>
    [Obsolete("Use TrySelect and throw if needed when the generator is not found.")]
    ValueGenerator? Select(IProperty property, ITypeBase typeBase);

    /// <summary>
    ///     Selects the appropriate value generator for a given property, if available.
    /// </summary>
    /// <param name="property">The property to get the value generator for.</param>
    /// <param name="typeBase">
    ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
    ///     this entity type may be different from the declared entity type on <paramref name="property" />
    /// </param>
    /// <param name="valueGenerator">The value generator, or <see langword="null"/> if none is available.</param>
    /// <returns><see langword="true"/> if a value generator was selected; <see langword="false"/> if none was available.</returns>
    bool TrySelect(IProperty property, ITypeBase typeBase, out ValueGenerator? valueGenerator);
}
