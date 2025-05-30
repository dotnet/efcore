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
public sealed class ClrPropertyGetter<TEntity, TStructural, TValue> : IClrPropertyGetter
    where TEntity : class
{
    private readonly Func<TEntity, IReadOnlyList<int>, TValue> _getClrValueUsingContainingEntity;
    private readonly Func<TEntity, IReadOnlyList<int>, bool> _hasSentinelValueUsingContainingEntity;
    private readonly Func<TStructural, TValue> _getClrValue;
    private readonly Func<TStructural, bool> _hasSentinelValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ClrPropertyGetter(
        Func<TEntity, IReadOnlyList<int>, TValue> getClrValueUsingContainingEntity,
        Func<TEntity, IReadOnlyList<int>, bool> hasSentinelValueUsingContainingEntity,
        Func<TStructural, TValue> getClrValue,
        Func<TStructural, bool> hasSentinelValue)
    {
        _getClrValueUsingContainingEntity = getClrValueUsingContainingEntity;
        _hasSentinelValueUsingContainingEntity = hasSentinelValueUsingContainingEntity;
        _getClrValue = getClrValue;
        _hasSentinelValue = hasSentinelValue;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetClrValueUsingContainingEntity(object entity, IReadOnlyList<int> indices)
        => _getClrValueUsingContainingEntity((TEntity)entity, indices);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetClrValueUsingContainingEntity(TEntity entity, IReadOnlyList<int> indices)
        => _getClrValueUsingContainingEntity(entity, indices);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSentinelValueUsingContainingEntity(object entity, IReadOnlyList<int> indices)
        => _hasSentinelValueUsingContainingEntity((TEntity)entity, indices);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSentinelUsingContainingEntity(TEntity entity, IReadOnlyList<int> indices)
        => HasSentinelValueUsingContainingEntity(entity, indices);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetClrValue(object structuralObject)
        => _getClrValue((TStructural)structuralObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetClrValue(TStructural structuralObject)
        => _getClrValue(structuralObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSentinelValue(object structuralObject)
        => _hasSentinelValue((TStructural)structuralObject);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasSentinelValue(TStructural structuralObject)
        => _hasSentinelValue(structuralObject);
}
