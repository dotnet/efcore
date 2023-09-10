// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
// Sealed for perf
public sealed class ClrPropertyGetter<TEntity, TStructuralType, TValue> : IClrPropertyGetter
    where TEntity : class
{
    private readonly Func<TEntity, TValue> _getter;
    private readonly Func<TEntity, bool> _hasSentinelValue;
    private readonly Func<TStructuralType, TValue> _structuralTypeGetter;
    private readonly Func<TStructuralType, bool> _hasStructuralTypeSentinelValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ClrPropertyGetter(
        Func<TEntity, TValue> getter,
        Func<TEntity, bool> hasSentinelValue,
        Func<TStructuralType, TValue> structuralTypeGetter,
        Func<TStructuralType, bool> hasStructuralTypeSentinelValue)
    {
        _getter = getter;
        _hasSentinelValue = hasSentinelValue;
        _structuralTypeGetter = structuralTypeGetter;
        _hasStructuralTypeSentinelValue = hasStructuralTypeSentinelValue;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetClrValueUsingContainingEntity(object entity)
        => _getter((TEntity)entity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSentinelUsingContainingEntity(object entity)
        => _hasSentinelValue((TEntity)entity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetClrValue(object structuralObject)
        => _structuralTypeGetter((TStructuralType)structuralObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSentinel(object structuralObject)
        => _hasStructuralTypeSentinelValue((TStructuralType)structuralObject);
}
