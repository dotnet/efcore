// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
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

            foreach (var foreignKey in key.FindReferencingForeignKeys())
            {
                var factorySource = foreignKey as IDependentKeyValueFactorySource;
                if (factorySource != null)
                {
                    factorySource.DependentKeyValueFactory = dependentFactory.CreateSimple<TKey>(foreignKey);
                }
            }

            return new SimplePrincipalKeyValueFactory<TKey>(key.Properties.Single().GetPropertyAccessors());
        }

        private static CompositePrincipalKeyValueFactory CreateCompositeFactory(IKey key)
        {
            var dependentFactory = new DependentKeyValueFactoryFactory();

            foreach (var foreignKey in key.FindReferencingForeignKeys())
            {
                var factorySource = foreignKey as IDependentKeyValueFactorySource;
                if (factorySource != null)
                {
                    factorySource.DependentKeyValueFactory = dependentFactory.CreateComposite(foreignKey);
                }
            }

            return new CompositePrincipalKeyValueFactory(key);
        }
    }
}
