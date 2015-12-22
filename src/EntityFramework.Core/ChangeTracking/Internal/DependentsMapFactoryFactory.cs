// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class DependentsMapFactoryFactory : IdentityMapFactoryFactoryBase
    {
        public virtual Func<IDependentsMap> Create([NotNull] IForeignKey foreignKey)
            => (Func<IDependentsMap>)typeof(DependentsMapFactoryFactory).GetTypeInfo()
                .GetDeclaredMethods(nameof(CreateFactory)).Single()
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
