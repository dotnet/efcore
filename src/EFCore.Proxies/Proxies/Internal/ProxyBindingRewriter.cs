// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ProxyBindingRewriter : IModelFinalizingConvention
{
    private static readonly MethodInfo _createLazyLoadingProxyMethod
        = typeof(IProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(IProxyFactory.CreateLazyLoadingProxy))!;

    private static readonly PropertyInfo _lazyLoaderProperty
        = typeof(IProxyLazyLoader).GetProperty(nameof(IProxyLazyLoader.LazyLoader))!;

    private static readonly MethodInfo _createProxyMethod
        = typeof(IProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(IProxyFactory.CreateProxy))!;

    private readonly ConstructorBindingConvention _directBindingConvention;
    private readonly IProxyFactory _proxyFactory;
    private readonly ProxiesOptionsExtension? _options;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ProxyBindingRewriter(
        IProxyFactory proxyFactory,
        ProxiesOptionsExtension? options,
        LazyLoaderParameterBindingFactoryDependencies lazyLoaderParameterBindingFactoryDependencies,
        ProviderConventionSetBuilderDependencies conventionSetBuilderDependencies)
    {
        _proxyFactory = proxyFactory;
        _options = options;
        LazyLoaderParameterBindingFactoryDependencies = lazyLoaderParameterBindingFactoryDependencies;
        ConventionSetBuilderDependencies = conventionSetBuilderDependencies;
        _directBindingConvention = new ConstructorBindingConvention(conventionSetBuilderDependencies);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies ConventionSetBuilderDependencies { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual LazyLoaderParameterBindingFactoryDependencies LazyLoaderParameterBindingFactoryDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        if (_options?.UseProxies == true)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType.IsAbstract)
                {
                    continue;
                }

                if (clrType.IsSealed)
                {
                    throw new InvalidOperationException(ProxiesStrings.ItsASeal(entityType.DisplayName()));
                }

                var proxyType = _proxyFactory.CreateProxyType(_options, entityType);

                // WARNING: This code is EF internal; it should not be copied. See #10789 #14554
#pragma warning disable EF1001 // Internal EF Core API usage.
                var binding = ((EntityType)entityType).ConstructorBinding;
                if (binding == null)
                {
                    _directBindingConvention.ProcessModelFinalizing(modelBuilder, context);
                    binding = ((EntityType)entityType).ConstructorBinding!;
                }

                ((EntityType)entityType).SetConstructorBinding(
                    UpdateConstructorBindings(entityType, proxyType, binding),
                    ConfigurationSource.Convention);

                binding = ((EntityType)entityType).ServiceOnlyConstructorBinding;
                if (binding != null)
                {
                    ((EntityType)entityType).SetServiceOnlyConstructorBinding(
                        UpdateConstructorBindings(entityType, proxyType, binding),
                        ConfigurationSource.Convention);
                }
#pragma warning restore EF1001 // Internal EF Core API usage.

                foreach (var navigationBase in entityType.GetDeclaredNavigations()
                             .Concat<IConventionNavigationBase>(entityType.GetDeclaredSkipNavigations()))
                {
                    if (navigationBase.PropertyInfo == null)
                    {
                        throw new InvalidOperationException(
                            ProxiesStrings.FieldProperty(navigationBase.Name, entityType.DisplayName()));
                    }

                    if (_options.UseChangeTrackingProxies
                        && navigationBase.PropertyInfo.SetMethod?.IsReallyVirtual() == false)
                    {
                        throw new InvalidOperationException(
                            ProxiesStrings.NonVirtualProperty(navigationBase.Name, entityType.DisplayName()));
                    }

                    if (_options.UseLazyLoadingProxies)
                    {
                        if (!navigationBase.PropertyInfo.GetMethod!.IsReallyVirtual()
                            && (!(navigationBase is INavigation navigation
                                && navigation.ForeignKey.IsOwnership)))
                        {
                            throw new InvalidOperationException(
                                ProxiesStrings.NonVirtualProperty(navigationBase.Name, entityType.DisplayName()));
                        }

                        navigationBase.SetPropertyAccessMode(PropertyAccessMode.Field);
                    }
                }

                if (_options.UseChangeTrackingProxies)
                {
                    var indexerChecked = false;
                    foreach (var property in entityType.GetDeclaredProperties()
                                 .Where(p => !p.IsShadowProperty()))
                    {
                        if (property.IsIndexerProperty())
                        {
                            if (!indexerChecked)
                            {
                                indexerChecked = true;

                                if (!property.PropertyInfo!.SetMethod!.IsReallyVirtual())
                                {
                                    if (clrType.IsGenericType
                                        && clrType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                                        && clrType.GenericTypeArguments[0] == typeof(string))
                                    {
                                        if (entityType.GetProperties().Any(p => !p.IsPrimaryKey()))
                                        {
                                            throw new InvalidOperationException(
                                                ProxiesStrings.DictionaryCannotBeProxied(
                                                    clrType.ShortDisplayName(),
                                                    entityType.DisplayName(),
                                                    typeof(IDictionary<,>).MakeGenericType(clrType.GenericTypeArguments)
                                                        .ShortDisplayName()));
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException(
                                            ProxiesStrings.NonVirtualIndexerProperty(entityType.DisplayName()));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (property.PropertyInfo == null)
                            {
                                throw new InvalidOperationException(
                                    ProxiesStrings.FieldProperty(property.Name, entityType.DisplayName()));
                            }

                            if (property.PropertyInfo.SetMethod?.IsReallyVirtual() == false)
                            {
                                throw new InvalidOperationException(
                                    ProxiesStrings.NonVirtualProperty(property.Name, entityType.DisplayName()));
                            }
                        }
                    }
                }
            }
        }
    }

    private InstantiationBinding UpdateConstructorBindings(
        IConventionEntityType entityType,
        Type proxyType,
        InstantiationBinding binding)
    {
        if (_options?.UseLazyLoadingProxies == true)
        {
            foreach (var conflictingProperty in entityType.GetDerivedTypes()
                         .SelectMany(e => e.GetDeclaredServiceProperties().Where(p => p.ClrType == typeof(ILazyLoader)))
                         .ToList())
            {
                conflictingProperty.DeclaringEntityType.RemoveServiceProperty(conflictingProperty.Name);
            }

            var serviceProperty = entityType.GetServiceProperties()
                .FirstOrDefault(e => e.ClrType == typeof(ILazyLoader));
            if (serviceProperty == null)
            {
                serviceProperty = entityType.AddServiceProperty(_lazyLoaderProperty);
                serviceProperty.SetParameterBinding(
                    (ServiceParameterBinding)new LazyLoaderParameterBindingFactory(
                            LazyLoaderParameterBindingFactoryDependencies)
                        .Bind(
                            entityType,
                            typeof(ILazyLoader),
                            nameof(IProxyLazyLoader.LazyLoader)));
            }

            return new FactoryMethodBinding(
                _proxyFactory,
                _createLazyLoadingProxyMethod,
                new List<ParameterBinding>
                {
                    new ContextParameterBinding(typeof(DbContext)),
                    new EntityTypeParameterBinding(),
                    new DependencyInjectionParameterBinding(
                        typeof(ILazyLoader), typeof(ILazyLoader), (IPropertyBase)serviceProperty),
                    new ObjectArrayParameterBinding(binding.ParameterBindings)
                },
                proxyType);
        }

        return new FactoryMethodBinding(
            _proxyFactory,
            _createProxyMethod,
            new List<ParameterBinding>
            {
                new ContextParameterBinding(typeof(DbContext)),
                new EntityTypeParameterBinding(),
                new ObjectArrayParameterBinding(binding.ParameterBindings)
            },
            proxyType);
    }
}
