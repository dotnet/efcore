// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DependentKeyValueFactoryFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDependentKeyValueFactory<TKey> CreateSimple<TKey>(
        IForeignKey foreignKey,
        IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        where TKey : notnull
    {
        var dependentIsNullable = foreignKey.Properties[0].ClrType.IsNullableType();
        var principalIsNullable = foreignKey.PrincipalKey.Properties[0].ClrType.IsNullableType();

        if (dependentIsNullable)
        {
            return principalIsNullable
                ? new SimpleFullyNullableDependentKeyValueFactory<TKey>(foreignKey, principalKeyValueFactory)
                : (IDependentKeyValueFactory<TKey>)Activator.CreateInstance(
                    typeof(SimpleNullableDependentKeyValueFactory<>).MakeGenericType(
                        typeof(TKey)), foreignKey, principalKeyValueFactory)!;
        }

        return principalIsNullable
            ? (IDependentKeyValueFactory<TKey>)Activator.CreateInstance(
                typeof(SimpleNullablePrincipalDependentKeyValueFactory<,>).MakeGenericType(
                    typeof(TKey), typeof(TKey).UnwrapNullableType()), foreignKey, principalKeyValueFactory)!
            : new SimpleNonNullableDependentKeyValueFactory<TKey>(foreignKey, principalKeyValueFactory);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDependentKeyValueFactory<IReadOnlyList<object?>> CreateComposite(
        IForeignKey foreignKey,
        IPrincipalKeyValueFactory<IReadOnlyList<object?>> principalKeyValueFactory)
        => new CompositeDependentKeyValueFactory(foreignKey, principalKeyValueFactory);
}
