// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class WeakReferenceIdentityMapFactoryFactory : IdentityMapFactoryFactoryBase
    {
        public virtual Func<IWeakReferenceIdentityMap> Create([NotNull] IKey key)
            => (Func<IWeakReferenceIdentityMap>)typeof(WeakReferenceIdentityMapFactoryFactory).GetTypeInfo()
                .GetDeclaredMethods(nameof(CreateFactory)).Single()
                .MakeGenericMethod(GetKeyType(key))
                .Invoke(null, new object[] { key });

        [UsedImplicitly]
        private static Func<IWeakReferenceIdentityMap> CreateFactory<TKey>(IKey key)
            => () => new WeakReferenceIdentityMap<TKey>(key, key.GetPrincipalKeyValueFactory<TKey>());
    }
}
