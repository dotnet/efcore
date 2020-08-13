// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ProxyBindingRewriter : IModelFinalizedConvention
    {
        private static readonly MethodInfo _createLazyLoadingProxyMethod
            = typeof(IProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(IProxyFactory.CreateLazyLoadingProxy));

        private static readonly PropertyInfo _lazyLoaderProperty
            = typeof(IProxyLazyLoader).GetProperty(nameof(IProxyLazyLoader.LazyLoader));

        private readonly ConstructorBindingConvention _directBindingConvention;
        private readonly LazyLoaderParameterBindingFactoryDependencies _lazyLoaderParameterBindingFactoryDependencies;
        private readonly IProxyFactory _proxyFactory;
        private readonly ProxiesOptionsExtension _options;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ProxyBindingRewriter(
            [NotNull] IProxyFactory proxyFactory,
            [CanBeNull] ProxiesOptionsExtension options,
            [NotNull] LazyLoaderParameterBindingFactoryDependencies lazyLoaderParameterBindingFactoryDependencies,
            [NotNull] ProviderConventionSetBuilderDependencies conventionSetBuilderDependencies)
        {
            _proxyFactory = proxyFactory;
            _options = options;
            _lazyLoaderParameterBindingFactoryDependencies = lazyLoaderParameterBindingFactoryDependencies;
            _directBindingConvention = new ConstructorBindingConvention(conventionSetBuilderDependencies);
        }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            if (_options?.UseLazyLoadingProxies == true)
            {
                foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
                {
                    if (entityType.ClrType?.IsAbstract == false)
                    {
                        if (entityType.ClrType.IsSealed)
                        {
                            throw new InvalidOperationException(ProxiesStrings.ItsASeal(entityType.DisplayName()));
                        }

                        var proxyType = _proxyFactory.CreateLazyLoadingProxyType(entityType);

                        foreach (var conflictingProperty in entityType.GetDerivedTypes()
                            .SelectMany(e => e.GetDeclaredServiceProperties().Where(p => p.ClrType == typeof(ILazyLoader)))
                            .ToList())
                        {
                            conflictingProperty.DeclaringEntityType.RemoveServiceProperty(conflictingProperty.Name);
                        }

                        var serviceProperty = entityType.GetServiceProperties().FirstOrDefault(e => e.ClrType == typeof(ILazyLoader));
                        if (serviceProperty == null)
                        {
                            serviceProperty = entityType.AddServiceProperty(_lazyLoaderProperty);
                            serviceProperty.SetParameterBinding(
                                (ServiceParameterBinding)new LazyLoaderParameterBindingFactory(
                                        _lazyLoaderParameterBindingFactoryDependencies)
                                    .Bind(
                                        entityType,
                                        typeof(ILazyLoader),
                                        nameof(IProxyLazyLoader.LazyLoader)));
                        }

                        // WARNING: This code is EF internal; it should not be copied. See #10789 #14554
                        var binding = (InstantiationBinding)entityType[CoreAnnotationNames.ConstructorBinding];
                        if (binding == null)
                        {
                            _directBindingConvention.ProcessModelFinalized(modelBuilder, context);
                        }

                        // WARNING: This code is EF internal; it should not be copied. See #10789 #14554
                        binding = (InstantiationBinding)entityType[CoreAnnotationNames.ConstructorBinding];

                        entityType.SetAnnotation(
                            // WARNING: This code is EF internal; it should not be copied. See #10789 #14554
                            CoreAnnotationNames.ConstructorBinding,
                            new FactoryMethodBinding(
                                _proxyFactory,
                                _createLazyLoadingProxyMethod,
                                new List<ParameterBinding>
                                {
                                    new EntityTypeParameterBinding(),
                                    new DependencyInjectionParameterBinding(typeof(ILazyLoader), typeof(ILazyLoader), serviceProperty),
                                    new ObjectArrayParameterBinding(binding.ParameterBindings)
                                },
                                proxyType));

                        foreach (var navigation in entityType.GetNavigations())
                        {
                            if (navigation.PropertyInfo == null)
                            {
                                throw new InvalidOperationException(
                                    ProxiesStrings.FieldNavigation(navigation.Name, entityType.DisplayName()));
                            }

                            if (!navigation.PropertyInfo.GetMethod.IsVirtual)
                            {
                                throw new InvalidOperationException(
                                    ProxiesStrings.NonVirtualNavigation(navigation.Name, entityType.DisplayName()));
                            }

                            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
                        }
                    }
                }
            }
        }
    }
}
