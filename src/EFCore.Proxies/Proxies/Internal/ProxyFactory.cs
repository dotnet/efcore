// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore.Internal;
using IInterceptor = Castle.DynamicProxy.IInterceptor;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ProxyFactory : IProxyFactory
{
    private static readonly Type ProxyLazyLoaderInterface = typeof(IProxyLazyLoader);
    private static readonly Type NotifyPropertyChangedInterface = typeof(INotifyPropertyChanged);
    private static readonly Type NotifyPropertyChangingInterface = typeof(INotifyPropertyChanging);

    private readonly ProxyGenerator _generator = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Create(
        DbContext context,
        Type type,
        params object[] constructorArguments)
    {
        var entityType = context.Model.FindRuntimeEntityType(type);
        if (entityType == null)
        {
            if (context.Model.IsShared(type))
            {
                throw new InvalidOperationException(ProxiesStrings.EntityTypeNotFoundShared(type.ShortDisplayName()));
            }

            throw new InvalidOperationException(CoreStrings.EntityTypeNotFound(type.ShortDisplayName()));
        }

        return CreateProxy(context, entityType, constructorArguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type CreateProxyType(
        ProxiesOptionsExtension options,
        IReadOnlyEntityType entityType)
        => _generator.ProxyBuilder.CreateClassProxyType(
            entityType.ClrType,
            GetInterfacesToProxy(options, entityType.ClrType),
            ProxyGenerationOptions.Default);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreateLazyLoadingProxy(
        DbContext context,
        IEntityType entityType,
        ILazyLoader loader,
        object[] constructorArguments)
    {
        var options = context.GetService<IDbContextOptions>().FindExtension<ProxiesOptionsExtension>();
        if (options == null)
        {
            throw new InvalidOperationException(ProxiesStrings.ProxyServicesMissing);
        }

        return CreateLazyLoadingProxy(
            options,
            entityType,
            context.GetService<ILazyLoader>(),
            constructorArguments);
    }

    private object CreateLazyLoadingProxy(
        ProxiesOptionsExtension options,
        IEntityType entityType,
        ILazyLoader loader,
        object[] constructorArguments)
        => _generator.CreateClassProxy(
            entityType.ClrType,
            GetInterfacesToProxy(options, entityType.ClrType),
            ProxyGenerationOptions.Default,
            constructorArguments,
            GetNotifyChangeInterceptors(options, entityType, new LazyLoadingInterceptor(entityType, loader)));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreateProxy(
        DbContext context,
        IEntityType entityType,
        object[] constructorArguments)
    {
        var options = context.GetService<IDbContextOptions>().FindExtension<ProxiesOptionsExtension>();
        if (options == null)
        {
            throw new InvalidOperationException(ProxiesStrings.ProxyServicesMissing);
        }

        if (options.UseLazyLoadingProxies)
        {
            return CreateLazyLoadingProxy(
                options,
                entityType,
                context.GetService<ILazyLoader>(),
                constructorArguments);
        }

        return CreateProxy(
            options,
            entityType,
            constructorArguments);
    }

    private object CreateProxy(
        ProxiesOptionsExtension options,
        IEntityType entityType,
        object[] constructorArguments)
        => _generator.CreateClassProxy(
            entityType.ClrType,
            GetInterfacesToProxy(options, entityType.ClrType),
            ProxyGenerationOptions.Default,
            constructorArguments,
            GetNotifyChangeInterceptors(options, entityType));

    private static Type[] GetInterfacesToProxy(
        ProxiesOptionsExtension options,
        Type type)
    {
        var interfacesToProxy = new List<Type>();

        if (options.UseLazyLoadingProxies)
        {
            interfacesToProxy.Add(ProxyLazyLoaderInterface);
        }

        if (options.UseChangeTrackingProxies)
        {
            if (!NotifyPropertyChangedInterface.IsAssignableFrom(type))
            {
                interfacesToProxy.Add(NotifyPropertyChangedInterface);
            }

            if (!NotifyPropertyChangingInterface.IsAssignableFrom(type))
            {
                interfacesToProxy.Add(NotifyPropertyChangingInterface);
            }
        }

        return interfacesToProxy.ToArray();
    }

    private static IInterceptor[] GetNotifyChangeInterceptors(
        ProxiesOptionsExtension options,
        IEntityType entityType,
        LazyLoadingInterceptor? lazyLoadingInterceptor = null)
    {
        var interceptors = new List<IInterceptor>();

        if (lazyLoadingInterceptor != null)
        {
            interceptors.Add(lazyLoadingInterceptor);
        }

        if (options.UseChangeTrackingProxies)
        {
            if (!NotifyPropertyChangedInterface.IsAssignableFrom(entityType.ClrType))
            {
                interceptors.Add(new PropertyChangedInterceptor(entityType, options.CheckEquality));
            }

            if (!NotifyPropertyChangingInterface.IsAssignableFrom(entityType.ClrType))
            {
                interceptors.Add(new PropertyChangingInterceptor(entityType, options.CheckEquality));
            }
        }

        return interceptors.ToArray();
    }
}
