// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class RowForeignKeyValueFactory<TKey> : IRowForeignKeyValueFactory<TKey>
{
    private readonly IForeignKeyConstraint _foreignKey;
    private readonly IRowKeyValueFactory<TKey> _principalKeyValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RowForeignKeyValueFactory(IForeignKeyConstraint foreignKey)
    {
        _foreignKey = foreignKey;
        _principalKeyValueFactory = (IRowKeyValueFactory<TKey>)((UniqueConstraint)foreignKey.PrincipalUniqueConstraint).GetRowKeyValueFactory();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEqualityComparer<TKey> EqualityComparer { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreatePrincipalValueIndex(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => new ValueIndex<TKey>(
            _foreignKey,
            _principalKeyValueFactory.CreateKeyValue(command, fromOriginalValues),
            EqualityComparer);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateDependentValueIndex(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => TryCreateDependentKeyValue(command, fromOriginalValues, out var keyValue)
            ? new ValueIndex<TKey>(_foreignKey, keyValue, EqualityComparer)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateDependentKeyValue(
        object?[] keyValues, [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateDependentKeyValue(
        IDictionary<string, object?> keyPropertyValues, [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateDependentKeyValue(
        IReadOnlyModificationCommand command, bool fromOriginalValues, [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEqualityComparer<TKey> CreateKeyEqualityComparer(IColumn column)
#pragma warning disable EF1001 // Internal EF Core API usage.
        => NullableComparerAdapter<TKey>.Wrap(column.ProviderValueComparer);
#pragma warning restore EF1001 // Internal EF Core API usage.

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[] CreatePrincipalKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => new object[] { _principalKeyValueFactory.CreateKeyValue(command, fromOriginalValues)! };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[]? CreateDependentKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => TryCreateDependentKeyValue(command, fromOriginalValues, out var value)
            ? (new object[] { value })
            : null;
}
