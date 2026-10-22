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
public class KeyValueFactoryFactory
{
    private static MethodInfo? _createSimpleFactoryNullableMethod;
    private static MethodInfo? _createSimpleFactoryNonNullableMethod;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IPrincipalKeyValueFactory Create(IKey key)
    {
        if (key.Properties.Count != 1)
        {
            return CreateCompositeFactory(key);
        }

        var keyType = key.GetKeyType();
        if (keyType.IsNullableType())
        {
            _createSimpleFactoryNullableMethod ??= typeof(KeyValueFactoryFactory).GetMethod(nameof(CreateSimpleNullableFactory))!;
            var nonNullableKeyType = keyType.UnwrapNullableType();
            return (IPrincipalKeyValueFactory)_createSimpleFactoryNullableMethod.MakeGenericMethod(
                    keyType, nonNullableKeyType == keyType ? typeof(int) : nonNullableKeyType)
                .Invoke(null, [key])!;
        }

        _createSimpleFactoryNonNullableMethod ??= typeof(KeyValueFactoryFactory)
            .GetMethod(nameof(CreateSimpleNonNullableFactory))!;
        return (IPrincipalKeyValueFactory)_createSimpleFactoryNonNullableMethod.MakeGenericMethod(keyType)
            .Invoke(null, [key])!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SimplePrincipalKeyValueFactory<TKey> CreateSimpleNullableFactory<TKey, TNonNullableKey>(IKey key)
        where TKey : notnull
        where TNonNullableKey : struct
    {
        DependentKeyValueFactoryFactory? dependentFactory = null;
        var principalKeyValueFactory = new SimplePrincipalKeyValueFactory<TKey>(key);

        foreach (var foreignKey in key.GetReferencingForeignKeys())
        {
            dependentFactory ??= new DependentKeyValueFactoryFactory();
            SetFactories(
                foreignKey,
                principalKeyValueFactory,
                dependentFactory.CreateSimpleNullable<TKey, TNonNullableKey>(foreignKey, principalKeyValueFactory));
        }

        return principalKeyValueFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SimplePrincipalKeyValueFactory<TKey> CreateSimpleNonNullableFactory<TKey>(IKey key)
        where TKey : struct
    {
        DependentKeyValueFactoryFactory? dependentFactory = null;
        var principalKeyValueFactory = new SimplePrincipalKeyValueFactory<TKey>(key);

        foreach (var foreignKey in key.GetReferencingForeignKeys())
        {
            dependentFactory ??= new DependentKeyValueFactoryFactory();
            SetFactories(
                foreignKey,
                principalKeyValueFactory,
                dependentFactory.CreateSimpleNonNullable(foreignKey, principalKeyValueFactory));
        }

        return principalKeyValueFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CompositePrincipalKeyValueFactory CreateCompositeFactory(IKey key)
    {
        DependentKeyValueFactoryFactory? dependentFactory = null;
        var principalKeyValueFactory = new CompositePrincipalKeyValueFactory(key);

        foreach (var foreignKey in key.GetReferencingForeignKeys())
        {
            dependentFactory ??= new DependentKeyValueFactoryFactory();
            SetFactories(
                foreignKey,
                principalKeyValueFactory,
                dependentFactory.CreateComposite(foreignKey, principalKeyValueFactory));
        }

        return principalKeyValueFactory;
    }

    private static void SetFactories<TKey>(
        IForeignKey foreignKey,
        IPrincipalKeyValueFactory<TKey> principalKeyValueFactory,
        IDependentKeyValueFactory<TKey> dependentKeyValueFactory)
        where TKey : notnull
    {
        var runtimeForeignKey = (IRuntimeForeignKey)foreignKey;
        runtimeForeignKey.DependentKeyValueFactory = dependentKeyValueFactory;
        runtimeForeignKey.DependentsMapFactory = () => new DependentsMap<TKey>(
            foreignKey, principalKeyValueFactory, dependentKeyValueFactory);
    }
}
