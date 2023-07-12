// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class DependentKeyValueFactory<TKey>
    where TKey : notnull
{
    private readonly IForeignKey _foreignKey;
    private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected DependentKeyValueFactory(
        IForeignKey foreignKey,
        IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
    {
        _foreignKey = foreignKey;
        _principalKeyValueFactory = principalKeyValueFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEqualityComparer<TKey> EqualityComparer { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateFromCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateFromOriginalValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreatePrincipalEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => new EquatableKeyValue<TKey>(
            _foreignKey,
            fromOriginalValues
                ? _principalKeyValueFactory.CreateFromOriginalValues(entry)!
                : _principalKeyValueFactory.CreateFromCurrentValues(entry)!,
            EqualityComparer);

    /// <summary>
    ///     Creates an equatable key object from the foreign key values in the given entry.
    /// </summary>
    /// <param name="entry">The entry tracking an entity instance.</param>
    /// <param name="fromOriginalValues">Whether the original or current values should be used.</param>
    /// <returns>The key object.</returns>
    public virtual object? CreateDependentEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => fromOriginalValues
            ? TryCreateFromOriginalValues(entry, out var originalKeyValue)
                ? new EquatableKeyValue<TKey>(_foreignKey, originalKeyValue, EqualityComparer)
                : null
            : TryCreateFromCurrentValues(entry, out var keyValue)
                ? new EquatableKeyValue<TKey>(_foreignKey, keyValue, EqualityComparer)
                : null;
}
