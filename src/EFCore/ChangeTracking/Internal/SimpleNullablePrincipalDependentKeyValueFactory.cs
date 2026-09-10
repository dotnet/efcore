// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// The methods here box, but this is only used when the primary key is nullable, but the FK is non-nullable,
// which is not common.
/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SimpleNullablePrincipalDependentKeyValueFactory<TKey, TNonNullableKey> : DependentKeyValueFactory<TKey>,
    IDependentKeyValueFactory<TKey>
    where TKey : notnull
    where TNonNullableKey : struct
{
    private readonly PropertyAccessors _propertyAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimpleNullablePrincipalDependentKeyValueFactory(
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
    public override bool TryCreateFromCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = (TKey)(object)((Func<IInternalEntry, TNonNullableKey>)_propertyAccessors.CurrentValueGetter)(
            (IInternalEntry)entry)!;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = (TKey)(object)((Func<IInternalEntry, TNonNullableKey>)_propertyAccessors.PreStoreGeneratedCurrentValueGetter)(
            (IInternalEntry)entry)!;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool TryCreateFromOriginalValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = (TKey)(object)((Func<IInternalEntry, TNonNullableKey>)_propertyAccessors.OriginalValueGetter!)(
            (IInternalEntry)entry)!;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromRelationshipSnapshot(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = (TKey)(object)((Func<IInternalEntry, TNonNullableKey>)_propertyAccessors.RelationshipSnapshotGetter)(
            (IInternalEntry)entry)!;
        return true;
    }
}
