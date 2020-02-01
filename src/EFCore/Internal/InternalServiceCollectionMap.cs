// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalServiceCollectionMap
    {
        private readonly IDictionary<Type, IList<int>> _serviceMap = new Dictionary<Type, IList<int>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalServiceCollectionMap([NotNull] IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;

            var index = 0;
            foreach (var descriptor in serviceCollection)
            {
                // ReSharper disable once VirtualMemberCallInConstructor
                GetOrCreateDescriptorIndexes(descriptor.ServiceType).Add(index++);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IServiceCollection ServiceCollection { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IList<int> GetOrCreateDescriptorIndexes([NotNull] Type serviceType)
        {
            if (!_serviceMap.TryGetValue(serviceType, out var indexes))
            {
                indexes = new List<int>();
                _serviceMap[serviceType] = indexes;
            }

            return indexes;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddNewDescriptor([NotNull] IList<int> indexes, [NotNull] ServiceDescriptor newDescriptor)
        {
            indexes.Add(ServiceCollection.Count);
            ServiceCollection.Add(newDescriptor);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalServiceCollectionMap AddDependencySingleton<TDependencies>()
            => AddDependency(typeof(TDependencies), ServiceLifetime.Singleton);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalServiceCollectionMap AddDependencyScoped<TDependencies>()
            => AddDependency(typeof(TDependencies), ServiceLifetime.Scoped);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalServiceCollectionMap AddDependency([NotNull] Type serviceType, ServiceLifetime lifetime)
        {
            var indexes = GetOrCreateDescriptorIndexes(serviceType);
            if (!indexes.Any())
            {
                AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, serviceType, lifetime));
            }
            else if (indexes.Count > 1
                || ServiceCollection[indexes[0]].ImplementationType != serviceType)
            {
                throw new InvalidOperationException(CoreStrings.BadDependencyRegistration(serviceType.Name));
            }

            return this;
        }

        /// <summary>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        ///     <para>
        ///         Re-writes the registration for the given service such that if the implementation type
        ///         implements <see cref="IPatchServiceInjectionSite" />, then
        ///         <see cref="IPatchServiceInjectionSite.InjectServices" /> will be called while resolving
        ///         the service allowing additional services to be injected without breaking the existing
        ///         constructor.
        ///     </para>
        ///     <para>
        ///         This mechanism should only be used to allow new services to be injected in a patch or
        ///         point release without making binary breaking changes.
        ///     </para>
        /// </summary>
        /// <typeparam name="TService"> The service contract. </typeparam>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual InternalServiceCollectionMap DoPatchInjection<TService>()
            where TService : class
        {
            if (_serviceMap.TryGetValue(typeof(TService), out var indexes))
            {
                foreach (var index in indexes)
                {
                    var descriptor = ServiceCollection[index];
                    var lifetime = descriptor.Lifetime;
                    var implementationType = descriptor.ImplementationType;

                    if (implementationType != null)
                    {
                        var implementationIndexes = GetOrCreateDescriptorIndexes(implementationType);
                        if (!implementationIndexes.Any())
                        {
                            AddNewDescriptor(
                                implementationIndexes,
                                new ServiceDescriptor(implementationType, implementationType, lifetime));
                        }

                        var injectedDescriptor = new ServiceDescriptor(
                            typeof(TService),
                            p => InjectServices(p, implementationType),
                            lifetime);

                        ServiceCollection[index] = injectedDescriptor;
                    }
                    else if (descriptor.ImplementationFactory != null)
                    {
                        var injectedDescriptor = new ServiceDescriptor(
                            typeof(TService),
                            p => InjectServices(p, descriptor.ImplementationFactory),
                            lifetime);

                        ServiceCollection[index] = injectedDescriptor;
                    }
                    else
                    {
                        var injectedDescriptor = new ServiceDescriptor(
                            typeof(TService),
                            p => InjectServices(p, descriptor.ImplementationInstance),
                            lifetime);

                        ServiceCollection[index] = injectedDescriptor;
                    }
                }
            }

            return this;
        }

        private static object InjectServices(IServiceProvider serviceProvider, Type concreteType)
        {
            var service = serviceProvider.GetService(concreteType);

            (service as IPatchServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }

        private static object InjectServices(IServiceProvider serviceProvider, object service)
        {
            (service as IPatchServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }

        private static object InjectServices(IServiceProvider serviceProvider, Func<IServiceProvider, object> implementationFactory)
        {
            var service = implementationFactory(serviceProvider);

            (service as IPatchServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }
    }
}
