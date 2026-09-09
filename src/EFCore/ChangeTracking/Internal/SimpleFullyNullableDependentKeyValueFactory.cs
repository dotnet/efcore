// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SimpleFullyNullableDependentKeyValueFactory<TKey> : DependentKeyValueFactory<TKey>, IDependentKeyValueFactory<TKey>
    where TKey : notnull
{
    private readonly PropertyAccessors _propertyAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimpleFullyNullableDependentKeyValueFactory(
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
    [Obsolete]
    public virtual bool TryCreateFromBuffer(in ValueBuffer valueBuffer, [NotNullWhen(true)] out TKey? key)
    {
        key = (TKey)_propertyAccessors.ValueBufferGetter!(valueBuffer);
        return key != null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool TryCreateFromCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.CurrentValueGetter)((InternalEntityEntry)entry);
        return key != null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromPreStoreGeneratedCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.PreStoreGeneratedCurrentValueGetter)((InternalEntityEntry)entry);
        return key != null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool TryCreateFromOriginalValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.OriginalValueGetter!)((InternalEntityEntry)entry);
        return key != null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateFromRelationshipSnapshot(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        key = ((Func<InternalEntityEntry, TKey>)_propertyAccessors.RelationshipSnapshotGetter)((InternalEntityEntry)entry);
        return key != null;
    }
}
