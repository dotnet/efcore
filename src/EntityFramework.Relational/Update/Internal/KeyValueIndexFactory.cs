// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class KeyValueIndexFactory<TKey> : IKeyValueIndexFactory
    {
        private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;

        public KeyValueIndexFactory([NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            _principalKeyValueFactory = principalKeyValueFactory;
        }

        public virtual IKeyValueIndex CreatePrincipalKeyValue(InternalEntityEntry entry, IForeignKey foreignKey)
            => new KeyValueIndex<TKey>(
                foreignKey,
                _principalKeyValueFactory.CreateFromCurrentValues(entry),
                _principalKeyValueFactory.EqualityComparer,
                fromOriginalValues: false);

        public virtual IKeyValueIndex CreatePrincipalKeyValueFromOriginalValues(InternalEntityEntry entry, IForeignKey foreignKey)
            => new KeyValueIndex<TKey>(
                foreignKey,
                _principalKeyValueFactory.CreateFromOriginalValues(entry),
                _principalKeyValueFactory.EqualityComparer,
                fromOriginalValues: true);

        public virtual IKeyValueIndex CreateDependentKeyValue(InternalEntityEntry entry, IForeignKey foreignKey)
        {
            TKey keyValue;
            return GetDependentKeyValueFactory(foreignKey).TryCreateFromCurrentValues(entry, out keyValue)
                ? new KeyValueIndex<TKey>(foreignKey, keyValue, _principalKeyValueFactory.EqualityComparer, fromOriginalValues: false)
                : null;
        }

        public virtual IKeyValueIndex CreateDependentKeyValueFromOriginalValues(InternalEntityEntry entry, IForeignKey foreignKey)
        {
            TKey keyValue;
            return GetDependentKeyValueFactory(foreignKey).TryCreateFromOriginalValues(entry, out keyValue)
                ? new KeyValueIndex<TKey>(foreignKey, keyValue, _principalKeyValueFactory.EqualityComparer, fromOriginalValues: true)
                : null;
        }

        private static IDependentKeyValueFactory<TKey> GetDependentKeyValueFactory(IForeignKey foreignKey)
        {
            var factorySource = foreignKey as IDependentKeyValueFactorySource;

            return factorySource != null
                ? (IDependentKeyValueFactory<TKey>)factorySource.DependentKeyValueFactory
                : new DependentKeyValueFactoryFactory().Create<TKey>(foreignKey);
        }
    }
}
