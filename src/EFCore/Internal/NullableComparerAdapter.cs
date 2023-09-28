// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class NullableComparerAdapter<TNullableKey> : IEqualityComparer<TNullableKey>
{
    private readonly IEqualityComparer _comparer;
    private readonly ValueConverter? _valueConverter;

    private NullableComparerAdapter(IEqualityComparer comparer, ValueConverter? valueConverter)
    {
        _comparer = comparer;
        _valueConverter = valueConverter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEqualityComparer<TNullableKey> Wrap(IEqualityComparer comparer, ValueConverter? valueConverter = null)
        => comparer as IEqualityComparer<TNullableKey>
            ?? new NullableComparerAdapter<TNullableKey>(comparer, valueConverter);

    /// <inheritdoc />
    public bool Equals(TNullableKey? x, TNullableKey? y)
        => (x is null && y is null)
            || (x is not null
                && y is not null
                && _comparer.Equals(
                    _valueConverter == null ? x : _valueConverter.ConvertToProvider(x),
                    _valueConverter == null ? y : _valueConverter.ConvertToProvider(y)));

    /// <inheritdoc />
    public int GetHashCode(TNullableKey obj)
        => obj is null
            ? 0
            : _comparer.GetHashCode(
                (_valueConverter == null
                    ? obj
                    : _valueConverter.ConvertToProvider(obj))!);
}
