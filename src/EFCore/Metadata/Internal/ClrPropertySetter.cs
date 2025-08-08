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
public sealed class ClrPropertySetter<TEntity, TStructural, TValue> : IClrPropertySetter
    where TEntity : class
{
    private readonly Action<TEntity, IReadOnlyList<int>, TValue> _setClrValueUsingContainingEntity;
    private readonly Func<TStructural, TValue, TStructural> _setClrValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ClrPropertySetter(Func<TStructural, TValue, TStructural> setClrValue)
    {
        Check.DebugAssert(typeof(TStructural) == typeof(TEntity), "TStructural should not be the same as TEntity");
        _setClrValue = setClrValue;
        _setClrValueUsingContainingEntity = (e, i, v) => _setClrValue((TStructural)(object)e, v);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ClrPropertySetter(Action<TEntity, IReadOnlyList<int>, TValue> setClrValueUsingContainingEntity, Func<TStructural, TValue, TStructural> setClrValue)
    {
        _setClrValueUsingContainingEntity = setClrValueUsingContainingEntity;
        _setClrValue = setClrValue;
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object SetClrValue(object instance, object? value)
    {
        if (_setClrValue != null)
        {
            return _setClrValue((TStructural)instance, (TValue)value!) ?? instance;
        }
        else
        {
            // Fallback to the containing entity method with empty indices
            _setClrValueUsingContainingEntity((TEntity)instance, [], (TValue)value!);
            return instance;
        }
    }
}
