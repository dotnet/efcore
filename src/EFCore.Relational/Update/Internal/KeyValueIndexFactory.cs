// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class KeyValueIndexFactory<TKey> : IKeyValueIndexFactory
{
    private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KeyValueIndexFactory(IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
    {
        _principalKeyValueFactory = principalKeyValueFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IKeyValueIndex CreatePrincipalKeyValue(IUpdateEntry entry, IForeignKey? foreignKey)
        => new KeyValueIndex<TKey>(
            foreignKey,
            _principalKeyValueFactory.CreateFromCurrentValues(entry),
            _principalKeyValueFactory.EqualityComparer,
            fromOriginalValues: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IKeyValueIndex CreatePrincipalKeyValueFromOriginalValues(IUpdateEntry entry, IForeignKey? foreignKey)
        => new KeyValueIndex<TKey>(
            foreignKey,
            _principalKeyValueFactory.CreateFromOriginalValues(entry),
            _principalKeyValueFactory.EqualityComparer,
            fromOriginalValues: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IKeyValueIndex? CreateDependentKeyValue(IUpdateEntry entry, IForeignKey foreignKey)
        => foreignKey.GetDependentKeyValueFactory<TKey>()!.TryCreateFromCurrentValues(entry, out var keyValue)
            ? new KeyValueIndex<TKey>(foreignKey, keyValue, _principalKeyValueFactory.EqualityComparer, fromOriginalValues: false)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IKeyValueIndex? CreateDependentKeyValueFromOriginalValues(IUpdateEntry entry, IForeignKey foreignKey)
        => foreignKey.GetDependentKeyValueFactory<TKey>()!.TryCreateFromOriginalValues(entry, out var keyValue)
            ? new KeyValueIndex<TKey>(foreignKey, keyValue, _principalKeyValueFactory.EqualityComparer, fromOriginalValues: true)
            : null;
}
