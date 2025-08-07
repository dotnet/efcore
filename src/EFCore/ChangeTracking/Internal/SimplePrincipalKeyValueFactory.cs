// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SimplePrincipalKeyValueFactory<TKey> : IPrincipalKeyValueFactory<TKey>
{
    private readonly IKey _key;
    private readonly PropertyAccessors _propertyAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimplePrincipalKeyValueFactory(IKey key)
    {
        _key = key;
        var property = key.Properties.Single();
        _propertyAccessors = property.GetPropertyAccessors();
        EqualityComparer = new NoNullsCustomEqualityComparer((ValueComparer<TKey>)property.GetKeyValueComparer());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateFromKeyValues(IReadOnlyList<object?> keyValues)
        => keyValues[0];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty FindNullPropertyInKeyValues(IReadOnlyList<object?> keyValues)
        => _key.Properties.Single();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromCurrentValues(IUpdateEntry entry)
        => ((Func<IInternalEntry, TKey>)_propertyAccessors.CurrentValueGetter)((IInternalEntry)entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty FindNullPropertyInCurrentValues(IUpdateEntry entry)
        => _key.Properties.Single();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromOriginalValues(IUpdateEntry entry)
        => ((Func<IInternalEntry, TKey>)_propertyAccessors.OriginalValueGetter!)((IInternalEntry)entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromRelationshipSnapshot(IUpdateEntry entry)
        => ((Func<IInternalEntry, TKey>)_propertyAccessors.RelationshipSnapshotGetter)((IInternalEntry)entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEqualityComparer<TKey> EqualityComparer { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreateEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => new EquatableKeyValue<TKey>(
            _key,
            fromOriginalValues
                ? CreateFromOriginalValues(entry)
                : CreateFromCurrentValues(entry),
            EqualityComparer);

    private sealed class NoNullsCustomEqualityComparer(ValueComparer<TKey> comparer) : IEqualityComparer<TKey>
    {
        public bool Equals(TKey? x, TKey? y)
            => comparer.Equals(x, y);

        public int GetHashCode([DisallowNull] TKey obj)
            => comparer.GetHashCode(obj);
    }
}
