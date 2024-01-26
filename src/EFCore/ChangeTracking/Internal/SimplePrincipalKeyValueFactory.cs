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
    private readonly IProperty _property;
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
        _property = key.Properties.Single();
        _propertyAccessors = _property.GetPropertyAccessors();

        EqualityComparer = new NoNullsCustomEqualityComparer((ValueComparer<TKey>)_property.GetKeyValueComparer());
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
    [Obsolete]
    public virtual object? CreateFromBuffer(ValueBuffer valueBuffer)
        => _propertyAccessors.ValueBufferGetter!(valueBuffer);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty FindNullPropertyInKeyValues(IReadOnlyList<object?> keyValues)
        => _property;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromCurrentValues(IUpdateEntry entry)
        => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.CurrentValueGetter)((InternalEntityEntry)entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IProperty FindNullPropertyInCurrentValues(IUpdateEntry entry)
        => _property;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromOriginalValues(IUpdateEntry entry)
        => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.OriginalValueGetter!)((InternalEntityEntry)entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TKey CreateFromRelationshipSnapshot(IUpdateEntry entry)
        => ((Func<InternalEntityEntry, TKey>)_propertyAccessors.RelationshipSnapshotGetter)((InternalEntityEntry)entry);

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

    private sealed class NoNullsCustomEqualityComparer : IEqualityComparer<TKey>
    {
        private readonly ValueComparer<TKey> _comparer;

        public NoNullsCustomEqualityComparer(ValueComparer<TKey> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(TKey? x, TKey? y)
            => _comparer.Equals(x, y);

        public int GetHashCode([DisallowNull] TKey obj)
            => _comparer.GetHashCode(obj);
    }
}
