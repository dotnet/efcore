// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

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
public class ValueGeneratorCache : IValueGeneratorCache
{
    private static readonly bool _useOldBehavior31539 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue31539", out var enabled31539) && enabled31539;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public ValueGeneratorCache(ValueGeneratorCacheDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ValueGeneratorCacheDependencies Dependencies { get; }

    private readonly ConcurrentDictionary<CacheKey, ValueGenerator> _cache = new();

    private readonly struct CacheKey : IEquatable<CacheKey>
    {
        private readonly Guid _modelId;
        private readonly string? _property;
        private readonly string? _typeBase;

        public CacheKey(IProperty property, ITypeBase typeBase)
        {
            if (_useOldBehavior31539)
            {
                _modelId = default;
                _property = null;
                _typeBase = null;
                Property = property;
                TypeBase = typeBase;
            }
            else
            {
                _modelId = typeBase.Model.ModelId;
                _property = property.Name;
                _typeBase = typeBase.Name;
                Property = null;
                TypeBase = null;
            }
        }

        public IProperty? Property { get; }

        public ITypeBase? TypeBase { get; }

        public bool Equals(CacheKey other)
            => _useOldBehavior31539
                ? Property!.Equals(other.Property) && TypeBase!.Equals(other.TypeBase)
                : (_property!.Equals(other._property, StringComparison.Ordinal)
                    && _typeBase!.Equals(other._typeBase, StringComparison.Ordinal)
                    && _modelId.Equals(other._modelId));

        public override bool Equals(object? obj)
            => obj is CacheKey cacheKey && Equals(cacheKey);

        public override int GetHashCode()
            => _useOldBehavior31539
                ? HashCode.Combine(Property!, TypeBase!)
                : HashCode.Combine(_property!, _typeBase!, _modelId);
    }

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
    public virtual ValueGenerator GetOrAdd(
        IProperty property,
        ITypeBase typeBase,
        Func<IProperty, ITypeBase, ValueGenerator> factory)
        => _cache.GetOrAdd(
                new CacheKey(property, typeBase), static (ck, p) => p.factory(p.property, p.typeBase), (factory, typeBase, property));
}
