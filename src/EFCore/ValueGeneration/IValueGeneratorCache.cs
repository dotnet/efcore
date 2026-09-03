// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     <para>
///         Keeps a cache of value generators for properties.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IValueGeneratorCache
{
    /// <summary>
    ///     Gets the existing value generator from the cache, or creates a new one if one is not present in
    ///     the cache.
    /// </summary>
    /// <param name="property">The property to get the value generator for.</param>
    /// <param name="typeBase">
    ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
    ///     this entity type may be different from the declared entity type on <paramref name="property" />
    /// </param>
    /// <param name="factory">Factory to create a new value generator if one is not present in the cache.</param>
    /// <returns>The existing or newly created value generator.</returns>
    ValueGenerator? GetOrAdd(
        IProperty property,
        ITypeBase typeBase,
        Func<IProperty, ITypeBase, ValueGenerator?> factory);
}
