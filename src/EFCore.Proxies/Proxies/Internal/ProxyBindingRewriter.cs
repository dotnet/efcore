// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
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
                    if (entityType.ClrType != null
                        && !entityType.ClrType.IsAbstract)
                    {
                        if (entityType.ClrType.IsSealed)
                        {
                            throw new InvalidOperationException(ProxiesStrings.ItsASeal(entityType.DisplayName()));
                        }

                        var binding = (ConstructorBinding)entityType[CoreAnnotationNames.ConstructorBinding];
                        if (binding == null)
                        {
                            _directBindingConvention.Apply(modelBuilder);
                        }

                        binding = (ConstructorBinding)entityType[CoreAnnotationNames.ConstructorBinding];

                        entityType[CoreAnnotationNames.ConstructorBinding] = RewriteToFactoryBinding(binding);

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

        private FactoryMethodConstructorBinding RewriteToFactoryBinding(ConstructorBinding currentBinding)
            => new FactoryMethodConstructorBinding(
                _proxyFactory,
                typeof(ProxyFactory).GetTypeInfo().GetDeclaredMethod(nameof(ProxyFactory.CreateLazyLoadingProxy)),
                new List<ParameterBinding>
                {
                    new EntityTypeParameterBinding(),
                    new ServiceParameterBinding(typeof(ILazyLoader), typeof(ILazyLoader)),
                    new ObjectArrayParameterBinding(currentBinding.ParameterBindings)
                }
            );
    }
}
