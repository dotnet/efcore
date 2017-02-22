// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IdentityMapFactoryFactory : IdentityMapFactoryFactoryBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<bool, IIdentityMap> Create([NotNull] IKey key)
            => (Func<bool, IIdentityMap>)typeof(IdentityMapFactoryFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))
                .MakeGenericMethod(GetKeyType(key))
                .Invoke(null, new object[] { key });

        [UsedImplicitly]
        private static Func<bool, IIdentityMap> CreateFactory<TKey>(IKey key)
        {
            var factory = key.GetPrincipalKeyValueFactory<TKey>();

            return typeof(TKey).IsNullableType()
                ? (Func<bool, IIdentityMap>)(sensitiveLoggingEnabled => new NullableKeyIdentityMap<TKey>(key, factory, sensitiveLoggingEnabled))
                : sensitiveLoggingEnabled => new IdentityMap<TKey>(key, factory, sensitiveLoggingEnabled);
        }
    }
}
