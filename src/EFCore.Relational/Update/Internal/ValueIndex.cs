// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class ValueIndex<TKey>
{
    private readonly object _metadata;
    private readonly TKey _keyValue;
    private readonly IEqualityComparer<TKey> _keyComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ValueIndex(
        object metadata,
        TKey keyValue,
        IEqualityComparer<TKey> keyComparer)
    {
        _metadata = metadata;
        _keyValue = keyValue;
        _keyComparer = keyComparer;
    }

    private bool Equals(ValueIndex<TKey> other)
        => other._metadata == _metadata
            && _keyComparer.Equals(_keyValue, other._keyValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj)
            || (obj is ValueIndex<TKey> other && Equals(other));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_metadata);
        hash.Add(_keyValue, _keyComparer);
        return hash.ToHashCode();
    }
}
