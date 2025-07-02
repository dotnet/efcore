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
public sealed class ClrPropertySetter<TEntity, TValue> : IClrPropertySetter
    where TEntity : class
{
    private readonly Action<TEntity, IReadOnlyList<int>, TValue> _setClrValueUsingContainingEntity;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ClrPropertySetter(Action<TEntity, IReadOnlyList<int>, TValue> setClrValueUsingContainingEntity)
    {
        _setClrValueUsingContainingEntity = setClrValueUsingContainingEntity;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClrValueUsingContainingEntity(object entity, IReadOnlyList<int> indices, object? value)
        => _setClrValueUsingContainingEntity((TEntity)entity, indices, (TValue)value!);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClrValueUsingContainingEntity(TEntity entity, IReadOnlyList<int> indices, TValue value)
        => _setClrValueUsingContainingEntity(entity, indices, value);
}
