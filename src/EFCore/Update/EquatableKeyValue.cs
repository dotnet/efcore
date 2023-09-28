// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     Objects of this type contain a key value corresponding to a Metadata item and implement <see cref="Equals(object?)" />
///     that return <see langword="true" /> only for other objects corresponding to the same Metadata item.
/// </summary>
/// <typeparam name="TKey">The underlying key type.</typeparam>
public sealed class EquatableKeyValue<TKey>
{
    private readonly IAnnotatable _metadata;
    private readonly TKey? _keyValue;
    private readonly IEqualityComparer<TKey> _keyComparer;

    /// <summary>
    ///     Creates a new instance of <see cref="EquatableKeyValue{TKey}" />
    /// </summary>
    /// <param name="metadata">The associated metadata.</param>
    /// <param name="keyValue">The underlying key value.</param>
    /// <param name="keyComparer">The key comparer.</param>
    public EquatableKeyValue(
        IAnnotatable metadata,
        TKey? keyValue,
        IEqualityComparer<TKey> keyComparer)
    {
        _metadata = metadata;
        _keyValue = keyValue;
        _keyComparer = keyComparer;
    }

    private bool Equals(EquatableKeyValue<TKey> other)
        => other._metadata == _metadata
            && _keyComparer.Equals(_keyValue, other._keyValue);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj)
            || (obj is EquatableKeyValue<TKey> other && Equals(other));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_metadata);
        if (_keyValue != null)
        {
            hash.Add(_keyValue, _keyComparer);
        }

        return hash.ToHashCode();
    }
}
