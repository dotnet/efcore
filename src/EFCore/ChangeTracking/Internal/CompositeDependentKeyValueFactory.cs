// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompositeDependentKeyValueFactory : CompositeValueFactory
{
    private readonly IForeignKey _foreignKey;
    private readonly IPrincipalKeyValueFactory<IReadOnlyList<object?>> _principalKeyValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompositeDependentKeyValueFactory(
        IForeignKey foreignKey,
        IPrincipalKeyValueFactory<IReadOnlyList<object?>> principalKeyValueFactory)
        : base(foreignKey.Properties)
    {
        _foreignKey = foreignKey;
        _principalKeyValueFactory = principalKeyValueFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object CreatePrincipalEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => new EquatableKeyValue<IReadOnlyList<object?>>(
            _foreignKey,
            fromOriginalValues
                ? _principalKeyValueFactory.CreateFromOriginalValues(entry)!
                : _principalKeyValueFactory.CreateFromCurrentValues(entry)!,
            EqualityComparer);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object? CreateDependentEquatableKey(IUpdateEntry entry, bool fromOriginalValues)
        => fromOriginalValues
            ? TryCreateFromOriginalValues(entry, out var originalKeyValue)
                ? new EquatableKeyValue<IReadOnlyList<object?>>(_foreignKey, originalKeyValue, EqualityComparer)
                : null
            : TryCreateFromCurrentValues(entry, out var keyValue)
                ? new EquatableKeyValue<IReadOnlyList<object?>>(_foreignKey, keyValue, EqualityComparer)
                : null;
}
