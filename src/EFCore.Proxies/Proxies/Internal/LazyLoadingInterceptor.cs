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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public LazyLoadingInterceptor(
            [NotNull] IEntityType entityType,
            [NotNull] ILazyLoader loader)
        {
            _entityType = entityType;
            _loader = loader;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                    var navigationBase = _entityType.FindNavigation(navigationName)
                        ?? (INavigationBase)_entityType.FindSkipNavigation(navigationName);

                    if (navigationBase != null
                        && (!(navigationBase is INavigation navigation
                            && navigation.ForeignKey.IsOwnership)))
                    {
                        _loader.Load(invocation.Proxy, navigationName);
                    }
                }

                invocation.Proceed();
            }
        }
    }
}
