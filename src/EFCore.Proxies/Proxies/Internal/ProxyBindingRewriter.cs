// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ProxyBindingRewriter : IModelFinalizingConvention
{
    private static readonly PropertyInfo LazyLoaderProperty
        = typeof(IProxyLazyLoader).GetProperty(nameof(IProxyLazyLoader.LazyLoader))!;

    private readonly ProxiesOptionsExtension? _options;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ProxyBindingRewriter(
        ProxiesOptionsExtension? options,
        LazyLoaderParameterBindingFactoryDependencies lazyLoaderParameterBindingFactoryDependencies,
        ProviderConventionSetBuilderDependencies conventionSetBuilderDependencies)
    {
        _options = options;
        LazyLoaderParameterBindingFactoryDependencies = lazyLoaderParameterBindingFactoryDependencies;
        ConventionSetBuilderDependencies = conventionSetBuilderDependencies;
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
            modelBuilder.HasAnnotation(ProxyAnnotationNames.LazyLoading, _options.UseLazyLoadingProxies);
            modelBuilder.HasAnnotation(ProxyAnnotationNames.ChangeTracking, _options.UseChangeTrackingProxies);
            modelBuilder.HasAnnotation(ProxyAnnotationNames.CheckEquality, _options.CheckEquality);

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

                foreach (var navigationBase in entityType.GetDeclaredNavigations()
                             .Concat<IConventionNavigationBase>(entityType.GetDeclaredSkipNavigations()))
                {
                    if (!navigationBase.IsShadowProperty())
                    {
                        if (_options.UseChangeTrackingProxies)
                        {
                            if (navigationBase.PropertyInfo == null)
                            {
                                throw new InvalidOperationException(
                                    ProxiesStrings.FieldProperty(navigationBase.Name, entityType.DisplayName()));
                            }

                            if (navigationBase.PropertyInfo.SetMethod?.IsReallyVirtual() == false)
                            {
                                throw new InvalidOperationException(
                                    ProxiesStrings.NonVirtualProperty(navigationBase.Name, entityType.DisplayName()));
                            }
                        }

                        if (_options.UseLazyLoadingProxies
                            && navigationBase.LazyLoadingEnabled)
                        {
                            if (navigationBase.PropertyInfo == null
                                || !navigationBase.PropertyInfo.GetMethod!.IsReallyVirtual())
                            {
                                if (!_options.IgnoreNonVirtualNavigations
                                    && navigationBase is not INavigation { ForeignKey.IsOwnership: true })
                                {
                                    if (navigationBase.PropertyInfo == null)
                                    {
                                        throw new InvalidOperationException(
                                            ProxiesStrings.FieldProperty(navigationBase.Name, entityType.DisplayName()));
                                    }

                                    throw new InvalidOperationException(
                                        ProxiesStrings.NonVirtualProperty(navigationBase.Name, entityType.DisplayName()));
                                }
                            }
                            else
                            {
                                navigationBase.SetPropertyAccessMode(PropertyAccessMode.Field);
                            }
                        }
                    }
                }

                if (_options.UseLazyLoadingProxies)
                {
                    foreach (var conflictingProperty in entityType.GetDerivedTypes()
                                 .SelectMany(e => e.GetDeclaredServiceProperties().Where(p => p.ClrType == typeof(ILazyLoader)))
                                 .ToList())
                    {
                        if (!ConfigurationSource.Convention.Overrides(conflictingProperty.GetConfigurationSource()))
                        {
                            break;
                        }

                        conflictingProperty.DeclaringEntityType.RemoveServiceProperty(conflictingProperty.Name);
                    }

                    var serviceProperty = entityType.GetServiceProperties()
                        .FirstOrDefault(e => e.ClrType == typeof(ILazyLoader));
                    if (serviceProperty == null)
                    {
                        serviceProperty = entityType.AddServiceProperty(LazyLoaderProperty);
                        serviceProperty.SetParameterBinding(
                            (ServiceParameterBinding)new LazyLoaderParameterBindingFactory(
                                    LazyLoaderParameterBindingFactoryDependencies)
                                .Bind(
                                    entityType,
                                    typeof(ILazyLoader),
                                    nameof(IProxyLazyLoader.LazyLoader)));
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
}
