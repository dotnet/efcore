// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LazyLoadingInterceptor : IInterceptor
    {
        private static readonly PropertyInfo _lazyLoaderProperty
            = typeof(IProxyLazyLoader).GetProperty(nameof(IProxyLazyLoader.LazyLoader));

        private static readonly MethodInfo _lazyLoaderGetter = _lazyLoaderProperty.GetMethod;
        private static readonly MethodInfo _lazyLoaderSetter = _lazyLoaderProperty.SetMethod;

        private readonly IEntityType _entityType;
        private ILazyLoader _loader;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public LazyLoadingInterceptor(
            [NotNull] IEntityType entityType,
            [NotNull] ILazyLoader loader)
        {
            _entityType = entityType;
            _loader = loader;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;

            if (_lazyLoaderGetter.Equals(invocation.Method))
            {
                invocation.ReturnValue = _loader;
            }
            else if (_lazyLoaderSetter.Equals(invocation.Method))
            {
                _loader = (ILazyLoader)invocation.Arguments[0];
            }
            else
            {
                if (_loader != null
                    && methodName.StartsWith("get_", StringComparison.Ordinal))
                {
                    var navigationName = methodName.Substring(4);
                    var navigation = _entityType.FindNavigation(navigationName);

                    if (navigation != null)
                    {
                        _loader.Load(invocation.Proxy, navigationName);
                    }
                }

                invocation.Proceed();
            }
        }
    }
}
