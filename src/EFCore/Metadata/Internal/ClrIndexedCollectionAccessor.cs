// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class ClrIndexedCollectionAccessor<TStructural, TCollection, TElement> : IClrIndexedCollectionAccessor
    where TCollection : class, IList<TElement>
{
    private readonly string _propertyName;
    private readonly bool _shadow;
    private readonly Func<TStructural, int, TElement>? _get;
    private readonly Action<TStructural, int, TElement?>? _set;
    private readonly Action<TStructural, int, TElement?>? _setForMaterialization;
    private readonly Func<int, TCollection> _createCollection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ClrIndexedCollectionAccessor(
        string propertyName,
        bool shadow,
        Func<TStructural, int, TElement>? get,
        Action<TStructural, int, TElement?>? set,
        Action<TStructural, int, TElement?>? setForMaterialization,
        Func<int, TCollection> createCollection)
    {
        _propertyName = propertyName;
        _shadow = shadow;
        _get = get;
        _set = set;
        _setForMaterialization = setForMaterialization;
        _createCollection = createCollection;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? Get(object entity, int index)
        => Get((TStructural)entity, index);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TElement? Get(TStructural entity, int index)
        => _shadow
            ? default
            : _get!(entity, index);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Set(object entity, int index, object? value, bool forMaterialization)
        => Set((TStructural)entity, index, (TElement?)value, forMaterialization);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void Set(TStructural entity, int index, TElement? value, bool forMaterialization)
    {
        var set = (forMaterialization ? _setForMaterialization : _set)
            ?? throw new InvalidOperationException(CoreStrings.NavigationNoSetter(_propertyName, typeof(TStructural).ShortDisplayName()));
        set(entity, index, value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object Create(int capacity)
        => _createCollection(capacity);
}
