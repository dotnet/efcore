// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Proxies.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ProxyBindingInterceptor : IInstantiationBindingInterceptor
{
    private static readonly MethodInfo CreateLazyLoadingProxyMethod
        = typeof(IProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(IProxyFactory.CreateLazyLoadingProxy))!;

    private static readonly MethodInfo CreateProxyMethod
        = typeof(IProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(IProxyFactory.CreateProxy))!;

    private readonly IProxyFactory _proxyFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ProxyBindingInterceptor(IProxyFactory proxyFactory)
    {
        _proxyFactory = proxyFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InstantiationBinding ModifyBinding(InstantiationBindingInterceptionData interceptionData, InstantiationBinding binding)
    {
        if (interceptionData.TypeBase is not IEntityType entityType)
        {
            return binding;
        }

        var proxyType = _proxyFactory.CreateProxyType(entityType);

        if ((bool?)entityType.Model[ProxyAnnotationNames.LazyLoading] == true)
        {
            var serviceProperty = entityType.GetServiceProperties()
                .First(e => e.ClrType == typeof(ILazyLoader));

            return new FactoryMethodBinding(
                _proxyFactory,
                CreateLazyLoadingProxyMethod,
                new List<ParameterBinding>
                {
                    new ContextParameterBinding(typeof(DbContext)),
                    new EntityTypeParameterBinding(),
                    new DependencyInjectionParameterBinding(typeof(ILazyLoader), typeof(ILazyLoader), serviceProperty),
                    new ObjectArrayParameterBinding(binding.ParameterBindings)
                },
                proxyType);
        }

        if ((bool?)entityType.Model[ProxyAnnotationNames.ChangeTracking] == true)
        {
            return new FactoryMethodBinding(
                _proxyFactory,
                CreateProxyMethod,
                new List<ParameterBinding>
                {
                    new ContextParameterBinding(typeof(DbContext)),
                    new EntityTypeParameterBinding(),
                    new ObjectArrayParameterBinding(binding.ParameterBindings)
                },
                proxyType);
        }

        return binding;
    }
}
