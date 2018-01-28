// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ProxyFactory : IProxyFactory
    {
        private readonly ProxyGenerator _generator = new ProxyGenerator();
        private static readonly Type[] _additionalInterfacesToProxy = { typeof(IProxyLazyLoader) };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Create(
            DbContext context,
            Type entityClrType,
            params object[] constructorArguments)
        {
            var entityType = context.Model.FindRuntimeEntityType(entityClrType);
            if (entityType == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotFound(entityClrType.ShortDisplayName()));
            }

            return CreateLazyLoadingProxy(entityType, context.GetService<ILazyLoader>(), constructorArguments);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type CreateLazyLoadingProxyType(IEntityType entityType)
            => _generator.ProxyBuilder.CreateClassProxyType(
                entityType.ClrType,
                _additionalInterfacesToProxy,
                ProxyGenerationOptions.Default);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateLazyLoadingProxy(
            IEntityType entityType,
            ILazyLoader loader,
            object[] constructorArguments)
            => _generator.CreateClassProxy(
                entityType.ClrType,
                _additionalInterfacesToProxy,
                ProxyGenerationOptions.Default,
                constructorArguments,
                new LazyLoadingInterceptor(entityType, loader));
    }
}
