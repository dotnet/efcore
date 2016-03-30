// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class KeyValueFactoryFactory
    {
        public virtual IPrincipalKeyValueFactory<TKey> Create<TKey>([NotNull] IKey key)
            => key.Properties.Count == 1
                ? CreateSimpleFactory<TKey>(key)
                : (IPrincipalKeyValueFactory<TKey>)CreateCompositeFactory(key);

        [UsedImplicitly]
        private static SimplePrincipalKeyValueFactory<TKey> CreateSimpleFactory<TKey>(IKey key)
        {
            var dependentFactory = new DependentKeyValueFactoryFactory();
            var principalKeyValueFactory
                = new SimplePrincipalKeyValueFactory<TKey>(key.Properties.Single().GetPropertyAccessors());

            foreach (var foreignKey in key.FindReferencingForeignKeys())
            {
                var dependentKeyValueFactory = dependentFactory.CreateSimple<TKey>(foreignKey);

                var factorySource = foreignKey as IDependentKeyValueFactorySource;
                if (factorySource != null)
                {
                    factorySource.DependentKeyValueFactory = dependentKeyValueFactory;
                }

                var mapSource = foreignKey as IDependentsMapFactorySource;
                if (mapSource != null)
                {
                    mapSource.DependentsMapFactory = () => new DependentsMap<TKey>(
                        foreignKey, principalKeyValueFactory, dependentKeyValueFactory);
                }
            }

            return principalKeyValueFactory;
        }

        private static CompositePrincipalKeyValueFactory CreateCompositeFactory(IKey key)
        {
            var dependentFactory = new DependentKeyValueFactoryFactory();
            var principalKeyValueFactory = new CompositePrincipalKeyValueFactory(key);

            foreach (var foreignKey in key.FindReferencingForeignKeys())
            {
                var dependentKeyValueFactory = dependentFactory.CreateComposite(foreignKey);

                var factorySource = foreignKey as IDependentKeyValueFactorySource;
                if (factorySource != null)
                {
                    factorySource.DependentKeyValueFactory = dependentKeyValueFactory;
                }

                var mapSource = foreignKey as IDependentsMapFactorySource;
                if (mapSource != null)
                {
                    mapSource.DependentsMapFactory = () => new DependentsMap<object[]>(
                        foreignKey, principalKeyValueFactory, dependentKeyValueFactory);
                }
            }

            return principalKeyValueFactory;
        }
    }
}
