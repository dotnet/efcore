// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Prvoides a map over a <see cref="IServiceCollection" /> that allows <see cref="ServiceDescriptor" />
    ///         entries to be conditionally added or re-written without requiring linear scans of the service
    ///         collection each time this is done.
    ///     </para>
    ///     <para>
    ///         Database providers are expected to create an instance of this around the service collection passed
    ///         to their 'Add...' method and then use the methods of this class to add services.
    ///     </para>
    ///     <para>
    ///         Note that the collection should not be modified without in other ways while it is being managed
    ///         by the map. The collection can be used in the normal way after modifications using the map have
    ///         been completed.
    ///     </para>
    /// </summary>
    public class ServiceCollectionMap
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IDictionary<Type, IList<int>> _serviceMap = new Dictionary<Type, IList<int>>();

        /// <summary>
        ///     Creates a new <see cref="ServiceCollectionMap" /> to operate on the given <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection"> The collection to work with. </param>
        public ServiceCollectionMap([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            _serviceCollection = serviceCollection;

            var index = 0;
            foreach (var descriptor in serviceCollection)
            {
                GetOrCreateDescriptorIndexes(descriptor.ServiceType).Add(index++);
            }
        }

        private IList<int> GetOrCreateDescriptorIndexes(Type serviceType)
        {
            IList<int> indexes;
            if (!_serviceMap.TryGetValue(serviceType, out indexes))
            {
                indexes = new List<int>();
                _serviceMap[serviceType] = indexes;
            }
            return indexes;
        }

        /// <summary>
        ///     The underlying <see cref="IServiceCollection" />.
        /// </summary>
        public virtual IServiceCollection ServiceCollection => _serviceCollection;

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransient([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAdd(serviceType, implementationType, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAdd(serviceType, implementationType, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAdd(serviceType, implementationType, ServiceLifetime.Singleton);

        private ServiceCollectionMap TryAdd(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementationType, nameof(implementationType));

            var indexes = GetOrCreateDescriptorIndexes(serviceType);
            if (!indexes.Any())
            {
                AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementationType, lifetime));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransient<TService>([NotNull] Func<IServiceProvider, TService> factory)
            where TService : class
            => TryAdd(typeof(TService), factory, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped<TService>([NotNull] Func<IServiceProvider, TService> factory)
            where TService : class
            => TryAdd(typeof(TService), factory, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton<TService>([NotNull] Func<IServiceProvider, TService> factory)
            where TService : class
            => TryAdd(typeof(TService), factory, ServiceLifetime.Singleton);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that the given factory creates. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransient<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory)
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), factory, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that the given factory creates. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory)
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), factory, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that the given factory creates. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory)
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), factory, ServiceLifetime.Singleton);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransient([NotNull] Type serviceType, [NotNull] Func<IServiceProvider, object> factory)
            => TryAdd(serviceType, factory, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped([NotNull] Type serviceType, [NotNull] Func<IServiceProvider, object> factory)
            => TryAdd(serviceType, factory, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton([NotNull] Type serviceType, [NotNull] Func<IServiceProvider, object> factory)
            => TryAdd(serviceType, factory, ServiceLifetime.Singleton);

        private ServiceCollectionMap TryAdd(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(factory, nameof(factory));

            var indexes = GetOrCreateDescriptorIndexes(serviceType);
            if (!indexes.Any())
            {
                AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, factory, lifetime));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton<TService>([CanBeNull] TService implementation)
            where TService : class
            => TryAdd(typeof(TService), implementation);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton([NotNull] Type serviceType, [CanBeNull] object implementation)
            => TryAdd(serviceType, implementation);

        private ServiceCollectionMap TryAdd(Type serviceType, object implementation)
        {
            Check.NotNull(serviceType, nameof(serviceType));

            var indexes = GetOrCreateDescriptorIndexes(serviceType);
            if (!indexes.Any())
            {
                AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementation));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given concrete
        ///     type to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransientEnumerable<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => TryAddEnumerable(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given concrete
        ///     type to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScopedEnumerable<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => TryAddEnumerable(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given concrete
        ///     type to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => TryAddEnumerable(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given concrete
        ///     type to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransientEnumerable([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given concrete
        ///     type to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScopedEnumerable([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given concrete
        ///     type to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Singleton);

        private ServiceCollectionMap TryAddEnumerable(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementationType, nameof(implementationType));

            var indexes = GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.All(i => TryGetImplementationType(_serviceCollection[i]) != implementationType))
            {
                AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementationType, lifetime));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given factory
        ///     to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <param name="factory"> The factory that implements this service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransientEnumerable<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory)
            where TService : class
            where TImplementation : class, TService
            => TryAddEnumerable<TService, TImplementation>(factory, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given factory
        ///     to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <param name="factory"> The factory that implements this service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScopedEnumerable<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory)
            where TService : class
            where TImplementation : class, TService
            => TryAddEnumerable<TService, TImplementation>(factory, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given factory
        ///     to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <param name="factory"> The factory that implements this service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory)
            where TService : class
            where TImplementation : class, TService
            => TryAddEnumerable<TService, TImplementation>(factory, ServiceLifetime.Singleton);

        private ServiceCollectionMap TryAddEnumerable<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory, ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            Check.NotNull(factory, nameof(factory));

            var indexes = GetOrCreateDescriptorIndexes(typeof(TService));
            if (indexes.All(i => TryGetImplementationType(_serviceCollection[i]) != typeof(TImplementation)))
            {
                AddNewDescriptor(indexes, new ServiceDescriptor(typeof(TService), factory, lifetime));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable<TService>([NotNull] TService implementation)
            where TService : class
            => TryAddEnumerable(typeof(TService), implementation);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     to ths list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable([NotNull] Type serviceType, [NotNull] object implementation)
            => TryAddEnumerable(serviceType, implementation);

        private ServiceCollectionMap TryAddEnumerable(Type serviceType, object implementation)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementation, nameof(implementation));

            var implementationType = implementation.GetType();

            var indexes = GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.All(i => TryGetImplementationType(_serviceCollection[i]) != implementationType))
            {
                AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementation));
            }

            return this;
        }

        private Type TryGetImplementationType(ServiceDescriptor descriptor)
            => descriptor.ImplementationType
               ?? descriptor.ImplementationInstance?.GetType()
               // Generic arg on Func may be obejct, but this is the best we can do and matches logic in D.I. container
               ?? descriptor.ImplementationFactory?.GetType().GetTypeInfo().GenericTypeArguments[1];

        private void AddNewDescriptor(IList<int> indexes, ServiceDescriptor newDescriptor)
        {
            indexes.Add(_serviceCollection.Count);
            _serviceCollection.Add(newDescriptor);
        }

        /// <summary>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        ///     <para>
        ///         Re-writes the registration for the gicen service such that if the implementation type
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
        public virtual ServiceCollectionMap DoPatchInjection<TService>()
            where TService : class
        {
            IList<int> indexes;
            if (_serviceMap.TryGetValue(typeof(TService), out indexes))
            {
                foreach (var index in indexes)
                {
                    var descriptor = _serviceCollection[index];
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

                        _serviceCollection[index] = injectedDescriptor;
                    }
                    else if (descriptor.ImplementationFactory != null)
                    {
                        var injectedDescriptor = new ServiceDescriptor(
                            typeof(TService),
                            p => InjectServices(p, descriptor.ImplementationFactory),
                            lifetime);

                        _serviceCollection[index] = injectedDescriptor;
                    }
                    else
                    {
                        var injectedDescriptor = new ServiceDescriptor(
                            typeof(TService),
                            p => InjectServices(p, descriptor.ImplementationInstance),
                            lifetime);

                        _serviceCollection[index] = injectedDescriptor;
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
