// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SimpleNullableDependentKeyValueFactory<TKey> : DependentKeyValueFactory<TKey>, IDependentKeyValueFactory<TKey>
    where TKey : struct
{
    private readonly PropertyAccessors _propertyAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimpleNullableDependentKeyValueFactory(
        IForeignKey foreignKey,
        IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        : base(foreignKey, principalKeyValueFactory)
    {
        var property = foreignKey.Properties.Single();
        _propertyAccessors = property.GetPropertyAccessors();
        EqualityComparer = property.CreateKeyEqualityComparer<TKey>();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEqualityComparer<TKey> EqualityComparer { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool TryCreateFromCurrentValues(IUpdateEntry entry, out TKey key)
        => HandleNullableValue(
            ((Func<IInternalEntry, TKey?>)_propertyAccessors.CurrentValueGetter)((IInternalEntry)entry), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(IUpdateEntry entry, out TKey key)
        => HandleNullableValue(
            ((Func<IInternalEntry, TKey?>)_propertyAccessors.PreStoreGeneratedCurrentValueGetter)((IInternalEntry)entry),
            out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool TryCreateFromOriginalValues(IUpdateEntry entry, out TKey key)
        => HandleNullableValue(
            ((Func<IInternalEntry, TKey?>)_propertyAccessors.OriginalValueGetter!)((IInternalEntry)entry), out key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromRelationshipSnapshot(IUpdateEntry entry, out TKey key)
        => HandleNullableValue(
            ((Func<IInternalEntry, TKey?>)_propertyAccessors.RelationshipSnapshotGetter)((InternalEntityEntry)entry), out key);

    private static bool HandleNullableValue(TKey? value, out TKey key)
    {
        if (value.HasValue)
        {
            key = (TKey)value;
            return true;
        }

        key = default;
        return false;
    }
}
