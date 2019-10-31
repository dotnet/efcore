// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    ///         Provides a map over a <see cref="IServiceCollection" /> that allows <see cref="ServiceDescriptor" />
    ///         entries to be conditionally added or re-written without requiring linear scans of the service
    ///         collection each time this is done.
    ///     </para>
    ///     <para>
    ///         Note that the collection should not be modified without in other ways while it is being managed
    ///         by the map. The collection can be used in the normal way after modifications using the map have
    ///         been completed.
    ///     </para>
    /// </summary>
    public class ServiceCollectionMap : IInfrastructure<InternalServiceCollectionMap>
    {
        private readonly InternalServiceCollectionMap _map;

        /// <summary>
        ///     Creates a new <see cref="ServiceCollectionMap" /> to operate on the given <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection"> The collection to work with. </param>
        public ServiceCollectionMap([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            _map = new InternalServiceCollectionMap(serviceCollection);
        }

        /// <summary>
        ///     The underlying <see cref="IServiceCollection" />.
        /// </summary>
        public virtual IServiceCollection ServiceCollection => _map.ServiceCollection;

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

        /// <summary>
        ///     Adds a service implemented by the given concrete type if no service for the given service
        ///     type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <param name="lifetime"> The service lifetime. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAdd(
            [NotNull] Type serviceType,
            [NotNull] Type implementationType,
            ServiceLifetime lifetime)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementationType, nameof(implementationType));

            var indexes = _map.GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.Count == 0)
            {
                _map.AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementationType, lifetime));
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

        /// <summary>
        ///     Adds a service implemented by the given factory if no service for the given service type
        ///     has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <param name="lifetime"> The service lifetime. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAdd(
            [NotNull] Type serviceType,
            [NotNull] Func<IServiceProvider, object> factory,
            ServiceLifetime lifetime)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(factory, nameof(factory));

            var indexes = _map.GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.Count == 0)
            {
                _map.AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, factory, lifetime));
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
            => TryAddSingleton(typeof(TService), implementation);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton([NotNull] Type serviceType, [CanBeNull] object implementation)
        {
            Check.NotNull(serviceType, nameof(serviceType));

            var indexes = _map.GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.Count == 0)
            {
                _map.AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementation));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given concrete
        ///     type to the list of services that implement the given contract. The service is only added
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
        ///     type to the list of services that implement the given contract. The service is only added
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
        ///     type to the list of services that implement the given contract. The service is only added
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
        ///     type to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddTransientEnumerable([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given concrete
        ///     type to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScopedEnumerable([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given concrete
        ///     type to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable([NotNull] Type serviceType, [NotNull] Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Singleton);

        /// <summary>
        ///     Adds a service implemented by the given concrete
        ///     type to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <param name="lifetime"> The service lifetime. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddEnumerable(
            [NotNull] Type serviceType,
            [NotNull] Type implementationType,
            ServiceLifetime lifetime)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementationType, nameof(implementationType));

            var indexes = _map.GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.All(i => TryGetImplementationType(ServiceCollection[i]) != implementationType))
            {
                _map.AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementationType, lifetime));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Transient" /> service implemented by the given factory
        ///     to the list of services that implement the given contract. The service is only added
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
            => TryAddEnumerable(typeof(TService), typeof(TImplementation), factory, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given factory
        ///     to the list of services that implement the given contract. The service is only added
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
            => TryAddEnumerable(typeof(TService), typeof(TImplementation), factory, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given factory
        ///     to the list of services that implement the given contract. The service is only added
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
            => TryAddEnumerable(typeof(TService), typeof(TImplementation), factory, ServiceLifetime.Singleton);

        /// <summary>
        ///     Adds a service implemented by the given factory
        ///     to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <param name="factory"> The factory that implements this service. </param>
        /// <param name="lifetime"> The service lifetime. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddEnumerable(
            [NotNull] Type serviceType,
            [NotNull] Type implementationType,
            [NotNull] Func<IServiceProvider, object> factory,
            ServiceLifetime lifetime)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementationType, nameof(implementationType));
            Check.NotNull(factory, nameof(factory));

            var indexes = _map.GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.All(i => TryGetImplementationType(ServiceCollection[i]) != implementationType))
            {
                _map.AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, factory, lifetime));
            }

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable<TService>([NotNull] TService implementation)
            where TService : class
            => TryAddSingletonEnumerable(typeof(TService), implementation);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable([NotNull] Type serviceType, [NotNull] object implementation)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementation, nameof(implementation));

            var implementationType = implementation.GetType();

            var indexes = _map.GetOrCreateDescriptorIndexes(serviceType);
            if (indexes.All(i => TryGetImplementationType(ServiceCollection[i]) != implementationType))
            {
                _map.AddNewDescriptor(indexes, new ServiceDescriptor(serviceType, implementation));
            }

            return this;
        }

        private static Type TryGetImplementationType(ServiceDescriptor descriptor)
            => descriptor.ImplementationType
                ?? descriptor.ImplementationInstance?.GetType()
                // Generic arg on Func may be object, but this is the best we can do and matches logic in D.I. container
                ?? descriptor.ImplementationFactory?.GetType().GetTypeInfo().GenericTypeArguments[1];

        InternalServiceCollectionMap IInfrastructure<InternalServiceCollectionMap>.Instance => _map;
    }
}
