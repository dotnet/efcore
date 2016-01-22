// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class KeyExtensions
    {
        public static Func<IIdentityMap> GetIdentityMapFactory([NotNull] this IKey key)
        {
            var factorySource = key as IIdentityMapFactorySource;

            return factorySource != null
                ? factorySource.IdentityMapFactory
                : new IdentityMapFactoryFactory().Create(key);
        }

        public static Func<IWeakReferenceIdentityMap> GetWeakReferenceIdentityMapFactory([NotNull] this IKey key)
        {
            var factorySource = key as IIdentityMapFactorySource;

            return factorySource != null
                ? factorySource.WeakReferenceIdentityMapFactory
                : new WeakReferenceIdentityMapFactoryFactory().Create(key);
        }

        public static IPrincipalKeyValueFactory<TKey> GetPrincipalKeyValueFactory<TKey>([NotNull] this IKey key)
        {
            var factorySource = key as IPrincipalKeyValueFactorySource;

            return factorySource != null
                ? factorySource.GetPrincipalKeyValueFactory<TKey>()
                : new KeyValueFactoryFactory().Create<TKey>(key);
        }

        public static bool IsPrimaryKey([NotNull] this IKey key)
        {
            Check.NotNull(key, nameof(key));

            return key == key.DeclaringEntityType.FindPrimaryKey();
        }
    }
}
