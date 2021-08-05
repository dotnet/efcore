// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    // This is a copy of https://github.com/OData/WebApi/blob/757631faf10b494e55117904196e8aa45d4c764d/src/Microsoft.AspNet.OData.Shared/DefaultContainerBuilder.cs
    // with a fix for reflection issue in BuildContainer
    // This can be removed once an OData package is available with https://github.com/OData/WebApi/commit/1528766b49b8dd49dd8845d8ea5e2973268eeee4
    public class WorkaroundContainerBuilder : IContainerBuilder
    {
        private readonly IServiceCollection services = new ServiceCollection();

        public virtual IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime, Type serviceType,Type implementationType)
        {
            services.Add(new ServiceDescriptor(
                serviceType ?? throw new ArgumentNullException(nameof(serviceType)),
                implementationType ?? throw new ArgumentNullException(nameof(implementationType)),
                TranslateServiceLifetime(lifetime)));

            return this;
        }

        public IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime, Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            services.Add(new ServiceDescriptor(
                serviceType ?? throw new ArgumentNullException(nameof(serviceType)),
                implementationFactory ?? throw new ArgumentNullException(nameof(implementationFactory)),
                TranslateServiceLifetime(lifetime)));

            return this;
        }

        public virtual IServiceProvider BuildContainer() => services.BuildServiceProvider(validateScopes: true);  // workaround is here, don't use reflection to call BuildServiceProvider

        private static Microsoft.Extensions.DependencyInjection.ServiceLifetime TranslateServiceLifetime(Microsoft.OData.ServiceLifetime lifetime) => lifetime switch
        {
            Microsoft.OData.ServiceLifetime.Scoped => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
            Microsoft.OData.ServiceLifetime.Singleton => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton,
            _ => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient
        };
    }
}
