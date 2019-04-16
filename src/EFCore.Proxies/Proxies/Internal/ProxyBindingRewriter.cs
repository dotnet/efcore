// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ProxyBindingRewriter : IModelBuiltConvention
    {
        private static readonly MethodInfo _createLazyLoadingProxyMethod
            = typeof(IProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(IProxyFactory.CreateLazyLoadingProxy));

        private static readonly PropertyInfo _lazyLoaderProperty
            = typeof(IProxyLazyLoader).GetProperty(nameof(IProxyLazyLoader.LazyLoader));

        private readonly ConstructorBindingConvention _directBindingConvention;
        private readonly IProxyFactory _proxyFactory;
        private readonly ProxiesOptionsExtension _options;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ProxyBindingRewriter(
            [NotNull] IProxyFactory proxyFactory,
            [NotNull] IConstructorBindingFactory bindingFactory,
            [CanBeNull] ProxiesOptionsExtension options)
        {
            _directBindingConvention = new ConstructorBindingConvention(bindingFactory);
            _proxyFactory = proxyFactory;
            _options = options;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
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
                            serviceProperty = entityType.AddServiceProperty(_lazyLoaderProperty, ConfigurationSource.Convention);
                            serviceProperty.SetParameterBinding(
                                (ServiceParameterBinding)new LazyLoaderParameterBindingFactory().Bind(
                                    entityType,
                                    typeof(ILazyLoader),
                                    nameof(IProxyLazyLoader.LazyLoader)));
                        }

                        var binding = (ConstructorBinding)entityType[CoreAnnotationNames.ConstructorBinding];
                        if (binding == null)
                        {
                            _directBindingConvention.Apply(modelBuilder);
                        }

                        binding = (ConstructorBinding)entityType[CoreAnnotationNames.ConstructorBinding];

                        entityType[CoreAnnotationNames.ConstructorBinding]
                            = new FactoryMethodConstructorBinding(
                                _proxyFactory,
                                _createLazyLoadingProxyMethod,
                                new List<ParameterBinding>
                                {
                                    new EntityTypeParameterBinding(),
                                    new DefaultServiceParameterBinding(typeof(ILazyLoader), typeof(ILazyLoader), serviceProperty),
                                    new ObjectArrayParameterBinding(binding.ParameterBindings)
                                },
                                proxyType);

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

            return modelBuilder;
        }
    }
}
