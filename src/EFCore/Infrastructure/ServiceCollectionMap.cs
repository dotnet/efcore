// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
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
    public class ServiceCollectionMap : IInfrastructure<IInternalServiceCollectionMap>
    {
        private readonly InternalServiceCollectionMap _map;

        /// <summary>
        ///     Creates a new <see cref="ServiceCollectionMap" /> to operate on the given <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection"> The collection to work with. </param>
        public ServiceCollectionMap(IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            _map = new InternalServiceCollectionMap(serviceCollection);
        }

        /// <summary>
        ///     The underlying <see cref="IServiceCollection" />.
        /// </summary>
        public virtual IServiceCollection ServiceCollection
            => _map.ServiceCollection;

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
        public virtual ServiceCollectionMap TryAddTransient(Type serviceType, Type implementationType)
            => TryAdd(serviceType, implementationType, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped(Type serviceType, Type implementationType)
            => TryAdd(serviceType, implementationType, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given concrete
        ///     type if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton(Type serviceType, Type implementationType)
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
            Type serviceType,
            Type implementationType,
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
        public virtual ServiceCollectionMap TryAddTransient<TService>(Func<IServiceProvider, TService> factory)
            where TService : class
            => TryAdd(typeof(TService), factory, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped<TService>(Func<IServiceProvider, TService> factory)
            where TService : class
            => TryAdd(typeof(TService), factory, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton<TService>(Func<IServiceProvider, TService> factory)
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
            Func<IServiceProvider, TImplementation> factory)
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
            Func<IServiceProvider, TImplementation> factory)
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
            Func<IServiceProvider, TImplementation> factory)
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
        public virtual ServiceCollectionMap TryAddTransient(Type serviceType, Func<IServiceProvider, object> factory)
            => TryAdd(serviceType, factory, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScoped(Type serviceType, Func<IServiceProvider, object> factory)
            => TryAdd(serviceType, factory, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given factory
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="factory"> The factory that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton(Type serviceType, Func<IServiceProvider, object> factory)
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
            Type serviceType,
            Func<IServiceProvider, object> factory,
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
        public virtual ServiceCollectionMap TryAddSingleton<TService>(TService implementation)
            where TService : class
            => TryAddSingleton(typeof(TService), implementation);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given instance
        ///     if no service for the given service type has already been registered.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementation"> The object that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingleton(Type serviceType, object implementation)
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
        public virtual ServiceCollectionMap TryAddTransientEnumerable(Type serviceType, Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Transient);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" /> service implemented by the given concrete
        ///     type to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddScopedEnumerable(Type serviceType, Type implementationType)
            => TryAddEnumerable(serviceType, implementationType, ServiceLifetime.Scoped);

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" /> service implemented by the given concrete
        ///     type to the list of services that implement the given contract. The service is only added
        ///     if the collection contains no other registration for the same service and implementation type.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> The map, such that further calls can be chained. </returns>
        public virtual ServiceCollectionMap TryAddSingletonEnumerable(Type serviceType, Type implementationType)
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
            Type serviceType,
            Type implementationType,
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
            Func<IServiceProvider, TImplementation> factory)
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
            Func<IServiceProvider, TImplementation> factory)
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
            Func<IServiceProvider, TImplementation> factory)
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
            Type serviceType,
            Type implementationType,
            Func<IServiceProvider, object> factory,
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
        public virtual ServiceCollectionMap TryAddSingletonEnumerable<TService>(TService implementation)
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
        public virtual ServiceCollectionMap TryAddSingletonEnumerable(Type serviceType, object implementation)
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
                ?? descriptor.ImplementationFactory?.GetType().GenericTypeArguments[1]!;

        IInternalServiceCollectionMap IInfrastructure<IInternalServiceCollectionMap>.Instance
            => _map;
    }
}
