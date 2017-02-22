// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class KeyValueFactoryFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IPrincipalKeyValueFactory<TKey> Create<TKey>([NotNull] IKey key)
            => key.Properties.Count == 1
                ? CreateSimpleFactory<TKey>(key)
                : (IPrincipalKeyValueFactory<TKey>)CreateCompositeFactory(key);

        [UsedImplicitly]
        private static SimplePrincipalKeyValueFactory<TKey> CreateSimpleFactory<TKey>(IKey key)
        {
            var dependentFactory = new DependentKeyValueFactoryFactory();
            var principalKeyValueFactory = new SimplePrincipalKeyValueFactory<TKey>(key.Properties.Single());

            foreach (var foreignKey in key.GetReferencingForeignKeys())
            {
                var dependentKeyValueFactory = dependentFactory.CreateSimple<TKey>(foreignKey);

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
                var dependentKeyValueFactory = dependentFactory.CreateComposite(foreignKey);

                SetFactories(
                    foreignKey,
                    dependentKeyValueFactory,
                    () => new DependentsMap<object[]>(foreignKey, principalKeyValueFactory, dependentKeyValueFactory));
            }

            return principalKeyValueFactory;
        }

        private static void SetFactories(
            IForeignKey foreignKey,
            object dependentKeyValueFactory,
            Func<IDependentsMap> dependentsMapFactory)
        {
            var conreteForeignKey = foreignKey.AsForeignKey();

            conreteForeignKey.DependentKeyValueFactory = dependentKeyValueFactory;
            conreteForeignKey.DependentsMapFactory = dependentsMapFactory;
        }
    }
}
