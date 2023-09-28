// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     The options to be used by a <see cref="DbContext" />. You normally override
///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder" />
///     to create instances of this class and it is not designed to be directly constructed in your application code.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
/// </remarks>
public abstract class DbContextOptions : IDbContextOptions
{
    private readonly ImmutableSortedDictionary<Type, (IDbContextOptionsExtension Extension, int Ordinal)> _extensionsMap;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected DbContextOptions()
    {
        _extensionsMap = ImmutableSortedDictionary.Create<Type, (IDbContextOptionsExtension, int)>(TypeFullNameComparer.Instance);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected DbContextOptions(
        IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
    {
        _extensionsMap = ImmutableSortedDictionary.Create<Type, (IDbContextOptionsExtension, int)>(TypeFullNameComparer.Instance)
            .AddRange(extensions.Select((p, i) => new KeyValuePair<Type, (IDbContextOptionsExtension, int)>(p.Key, (p.Value, i))));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected DbContextOptions(
        ImmutableSortedDictionary<Type, (IDbContextOptionsExtension Extension, int Ordinal)> extensions)
    {
        _extensionsMap = extensions;
    }

    /// <summary>
    ///     Gets the extensions that store the configured options.
    /// </summary>
    public virtual IEnumerable<IDbContextOptionsExtension> Extensions
        => _extensionsMap.Values.OrderBy(v => v.Ordinal).Select(v => v.Extension);

    /// <summary>
    ///     Gets the extension of the specified type. Returns <see langword="null" /> if no extension of the specified type is configured.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension to get.</typeparam>
    /// <returns>The extension, or <see langword="null" /> if none was found.</returns>
    public virtual TExtension? FindExtension<TExtension>()
        where TExtension : class, IDbContextOptionsExtension
        => _extensionsMap.TryGetValue(typeof(TExtension), out var value) ? (TExtension)value.Extension : null;

    /// <summary>
    ///     Gets the extension of the specified type. Throws if no extension of the specified type is configured.
    /// </summary>
    /// <typeparam name="TExtension">The type of the extension to get.</typeparam>
    /// <returns>The extension.</returns>
    public virtual TExtension GetExtension<TExtension>()
        where TExtension : class, IDbContextOptionsExtension
    {
        var extension = FindExtension<TExtension>();
        if (extension == null)
        {
            throw new InvalidOperationException(CoreStrings.OptionsExtensionNotFound(typeof(TExtension).ShortDisplayName()));
        }

        return extension;
    }

    /// <summary>
    ///     Adds the given extension to the underlying options and creates a new
    ///     <see cref="DbContextOptions" /> with the extension added.
    /// </summary>
    /// <typeparam name="TExtension">The type of extension to be added.</typeparam>
    /// <param name="extension">The extension to be added.</param>
    /// <returns>The new options instance with the given extension added.</returns>
    public abstract DbContextOptions WithExtension<TExtension>(TExtension extension)
        where TExtension : class, IDbContextOptionsExtension;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual ImmutableSortedDictionary<Type, (IDbContextOptionsExtension Extension, int Ordinal)> ExtensionsMap
        => _extensionsMap;

    /// <summary>
    ///     The type of context that these options are for. Will return <see cref="DbContext" /> if the
    ///     options are not built for a specific derived context.
    /// </summary>
    public abstract Type ContextType { get; }

    /// <summary>
    ///     Specifies that no further configuration of this options object should occur.
    /// </summary>
    public virtual void Freeze()
        => IsFrozen = true;

    /// <summary>
    ///     Returns <see langword="true" /> if <see cref="Freeze" /> has been called. A frozen options object cannot be further
    ///     configured with <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />.
    /// </summary>
    public virtual bool IsFrozen { get; private set; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj)
            || (obj is DbContextOptions otherOptions && Equals(otherOptions));

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>
    ///     <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    protected virtual bool Equals(DbContextOptions other)
        => _extensionsMap.Count == other._extensionsMap.Count
            && _extensionsMap.Zip(other._extensionsMap)
                .All(
                    p => p.First.Value.Ordinal == p.Second.Value.Ordinal
                        && p.First.Value.Extension.Info.ShouldUseSameServiceProvider(p.Second.Value.Extension.Info));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var (type, value) in _extensionsMap)
        {
            hashCode.Add(type);
            hashCode.Add(value.Extension.Info.GetServiceProviderHashCode());
        }

        return hashCode.ToHashCode();
    }
}
