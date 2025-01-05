// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Castle.DynamicProxy;
using IInterceptor = Castle.DynamicProxy.IInterceptor;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class LazyLoadingInterceptor : IInterceptor
{
    private static readonly PropertyInfo LazyLoaderProperty
        = typeof(IProxyLazyLoader).GetProperty(nameof(IProxyLazyLoader.LazyLoader))!;

    private static readonly MethodInfo LazyLoaderGetter = LazyLoaderProperty.GetMethod!;
    private static readonly MethodInfo LazyLoaderSetter = LazyLoaderProperty.SetMethod!;

    private ILazyLoader? _loader;
    private readonly HashSet<string> _navigations;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public LazyLoadingInterceptor(
        IEntityType entityType,
        ILazyLoader loader)
    {
        _loader = loader;
        _navigations = entityType!.GetNavigations().Where(n => !n.ForeignKey.IsOwnership)
            .Cast<INavigationBase>()
            .Concat(entityType.GetSkipNavigations())
            .Select(n => n.Name)
            .ToHashSet();
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

        if (LazyLoaderGetter.Equals(invocation.Method))
        {
            invocation.ReturnValue = _loader;
        }
        else if (LazyLoaderSetter.Equals(invocation.Method))
        {
            _loader = (ILazyLoader)invocation.Arguments[0];
        }
        else
        {
            if (_loader != null
                && methodName.StartsWith("get_", StringComparison.Ordinal))
            {
                var navigationName = methodName[4..];
                if (_navigations.Contains(navigationName))
                {
                    _loader.Load(invocation.Proxy, navigationName);
                }
            }

            invocation.Proceed();
        }
    }
}
