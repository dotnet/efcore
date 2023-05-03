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
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IPrincipalKeyValueFactory<TKey> Create<TKey>(IKey key)
        where TKey : notnull
        => key.Properties.Count == 1
            ? CreateSimpleFactory<TKey>(key)
            : (IPrincipalKeyValueFactory<TKey>)CreateCompositeFactory(key);

    private static SimplePrincipalKeyValueFactory<TKey> CreateSimpleFactory<TKey>(IKey key)
        where TKey : notnull
    {
        var dependentFactory = new DependentKeyValueFactoryFactory();
        var principalKeyValueFactory = new SimplePrincipalKeyValueFactory<TKey>(key);

        foreach (var foreignKey in key.GetReferencingForeignKeys())
        {
            var dependentKeyValueFactory = dependentFactory.CreateSimple(foreignKey, principalKeyValueFactory);

            SetFactories(
                foreignKey,
                dependentKeyValueFactory,
                () => new DependentsMap<TKey>(foreignKey, principalKeyValueFactory, dependentKeyValueFactory));
        }

        return principalKeyValueFactory;
    }

    private static CompositePrincipalKeyValueFactory CreateCompositeFactory(IKey key)
    {
        var dependentFactory = new DependentKeyValueFactoryFactory();
        var principalKeyValueFactory = new CompositePrincipalKeyValueFactory(key);

        foreach (var foreignKey in key.GetReferencingForeignKeys())
        {
            var dependentKeyValueFactory = dependentFactory.CreateComposite(foreignKey, principalKeyValueFactory);

            SetFactories(
                foreignKey,
                dependentKeyValueFactory,
                () => new DependentsMap<IReadOnlyList<object?>>(foreignKey, principalKeyValueFactory, dependentKeyValueFactory));
        }

        return principalKeyValueFactory;
    }

    private static void SetFactories(
        IForeignKey foreignKey,
        IDependentKeyValueFactory dependentKeyValueFactory,
        Func<IDependentsMap> dependentsMapFactory)
    {
        var concreteForeignKey = (IRuntimeForeignKey)foreignKey;

        concreteForeignKey.DependentKeyValueFactory = dependentKeyValueFactory;
        concreteForeignKey.DependentsMapFactory = dependentsMapFactory;
    }
}
