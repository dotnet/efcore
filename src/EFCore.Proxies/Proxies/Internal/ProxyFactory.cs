// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ProxyFactory : IProxyFactory
    {
        private static readonly Type _proxyLazyLoaderInterface = typeof(IProxyLazyLoader);
        private static readonly Type _notifyPropertyChangedInterface = typeof(INotifyPropertyChanged);
        private static readonly Type _notifyPropertyChangingInterface = typeof(INotifyPropertyChanging);

        private readonly IDbContextOptions _dbContextOptions;
        private readonly Lazy<ProxiesOptionsExtension> _options;
        private readonly ProxyGenerator _generator = new ProxyGenerator();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual ProxiesOptionsExtension Options => _options.Value;

        public ProxyFactory(
            [NotNull] IDbContextOptions dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
            _options = new Lazy<ProxiesOptionsExtension>(
                () =>
                {
                    var extension = _dbContextOptions.FindExtension<ProxiesOptionsExtension>();

                    if (extension == null)
                    {
                        throw new InvalidOperationException(ProxiesStrings.ProxyServicesMissing);
                    }

                    return extension;
                });
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

            if (Options.UseLazyLoadingProxies)
            {
                return CreateLazyLoadingProxy(entityType, context.GetService<ILazyLoader>(), constructorArguments);
            } 

            return CreateProxy(entityType, constructorArguments);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type CreateProxyType(IEntityType entityType)
            => _generator.ProxyBuilder.CreateClassProxyType(
                entityType.ClrType,
                GetInterfacesToProxy(entityType),
                ProxyGenerationOptions.Default);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object CreateLazyLoadingProxy(
            IEntityType entityType,
            ILazyLoader loader,
            object[] constructorArguments)
            => _generator.CreateClassProxy(
                entityType.ClrType,
                GetInterfacesToProxy(entityType),
                ProxyGenerationOptions.Default,
                constructorArguments,
                GetNotifyChangeInterceptors(entityType, new LazyLoadingInterceptor(entityType, loader)));

        private object CreateProxy(
            IEntityType entityType,
            object[] constructorArguments)
            => _generator.CreateClassProxy(
                entityType.ClrType,
                GetInterfacesToProxy(entityType),
                ProxyGenerationOptions.Default,
                constructorArguments,
                GetNotifyChangeInterceptors(entityType));

        private Type[] GetInterfacesToProxy(
            IEntityType entityType)
        {
            var interfacesToProxy = new List<Type>();

            if (Options.UseLazyLoadingProxies)
            {
                interfacesToProxy.Add(_proxyLazyLoaderInterface);
            }

            if (Options.UseChangeDetectionProxies)
            {
                var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();
                switch (changeTrackingStrategy)
                {
                    case ChangeTrackingStrategy.ChangedNotifications:
                        interfacesToProxy.Add(_notifyPropertyChangedInterface);
                        break;
                    case ChangeTrackingStrategy.ChangingAndChangedNotifications:
                    case ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues:
                        interfacesToProxy.Add(_notifyPropertyChangedInterface);
                        interfacesToProxy.Add(_notifyPropertyChangingInterface);
                        break;
                    default:
                        break;
                }
            }

            return interfacesToProxy.ToArray();
        }

        private Castle.DynamicProxy.IInterceptor[] GetNotifyChangeInterceptors(
            IEntityType entityType,
            LazyLoadingInterceptor lazyLoadingInterceptor = null)
        {
            var interceptors = new List<Castle.DynamicProxy.IInterceptor>();

            if (lazyLoadingInterceptor != null)
            {
                interceptors.Add(lazyLoadingInterceptor);
            }

            if (Options.UseChangeDetectionProxies)
            {
                var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();
                switch (changeTrackingStrategy)
                {
                    case ChangeTrackingStrategy.ChangedNotifications:
                        interceptors.Add(new PropertyChangedInterceptor(entityType, Options.CheckEquality));
                        break;
                    case ChangeTrackingStrategy.ChangingAndChangedNotifications:
                    case ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues:
                        interceptors.Add(new PropertyChangedInterceptor(entityType, Options.CheckEquality));
                        interceptors.Add(new PropertyChangingInterceptor(entityType, Options.CheckEquality));
                        break;
                    default:
                        break;
                }
            }

            return interceptors.ToArray();
        }
    }
}
