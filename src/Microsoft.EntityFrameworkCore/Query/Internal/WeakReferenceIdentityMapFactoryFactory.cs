// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class WeakReferenceIdentityMapFactoryFactory : IdentityMapFactoryFactoryBase
    {
        public virtual Func<IWeakReferenceIdentityMap> Create([NotNull] IKey key)
            => (Func<IWeakReferenceIdentityMap>)typeof(WeakReferenceIdentityMapFactoryFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))
                .MakeGenericMethod(GetKeyType(key))
                .Invoke(null, new object[] { key });

        [UsedImplicitly]
        private static Func<IWeakReferenceIdentityMap> CreateFactory<TKey>(IKey key)
            => () => new WeakReferenceIdentityMap<TKey>(key, key.GetPrincipalKeyValueFactory<TKey>());
    }
}
