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
    public class DependentsMapFactoryFactory : IdentityMapFactoryFactoryBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<IDependentsMap> Create([NotNull] IForeignKey foreignKey)
            => (Func<IDependentsMap>)typeof(DependentsMapFactoryFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))
                .MakeGenericMethod(GetKeyType(foreignKey.PrincipalKey))
                .Invoke(null, new object[] { foreignKey });

        [UsedImplicitly]
        private static Func<IDependentsMap> CreateFactory<TKey>(IForeignKey foreignKey)
        {
            var principalKeyValueFactory = foreignKey.PrincipalKey.GetPrincipalKeyValueFactory<TKey>();
            var dependentKeyValueFactory = foreignKey.GetDependentKeyValueFactory<TKey>();

            return () => new DependentsMap<TKey>(foreignKey, principalKeyValueFactory, dependentKeyValueFactory);
        }
    }
}
