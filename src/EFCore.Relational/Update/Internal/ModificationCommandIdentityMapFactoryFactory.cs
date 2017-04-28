// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public class ModificationCommandIdentityMapFactoryFactory : IdentityMapFactoryFactoryBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<IModificationCommandIdentityMap> Create([NotNull] IReadOnlyList<IEntityType> entityTypes)
            => (Func<IModificationCommandIdentityMap>)typeof(ModificationCommandIdentityMapFactoryFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))
                .MakeGenericMethod(GetKeyType(entityTypes[0].FindPrimaryKey()))
                .Invoke(null, new object[] { entityTypes });

        [UsedImplicitly]
        private static Func<IModificationCommandIdentityMap> CreateFactory<TKey>(IReadOnlyList<IEntityType> entityTypes)
            => () => new ModificationCommandIdentityMap<TKey>(entityTypes);
    }
}
