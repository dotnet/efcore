// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalServiceCollectionMap : IInternalServiceCollectionMap
{
    private readonly IDictionary<Type, IList<int>> _serviceMap = new Dictionary<Type, IList<int>>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalServiceCollectionMap(IServiceCollection serviceCollection)
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
    public virtual IList<int> GetOrCreateDescriptorIndexes(Type serviceType)
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
    public virtual void AddNewDescriptor(IList<int> indexes, ServiceDescriptor newDescriptor)
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
    public virtual IInternalServiceCollectionMap AddDependencySingleton<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDependencies>()
        => AddDependency(typeof(TDependencies), ServiceLifetime.Singleton);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IInternalServiceCollectionMap
        AddDependencyScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDependencies>()
        => AddDependency(typeof(TDependencies), ServiceLifetime.Scoped);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IInternalServiceCollectionMap AddDependency(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType,
        ServiceLifetime lifetime)
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
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
    /// </remarks>
    /// <typeparam name="TService">The service contract.</typeparam>
    /// <returns>The map, such that further calls can be chained.</returns>
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
                        // TODO: What should we do here? Can annotate InjectServices to accept null, but then it has to return it too...
                        p => InjectServices(p, descriptor.ImplementationInstance!),
                        lifetime);

                    ServiceCollection[index] = injectedDescriptor;
                }
            }
        }

        return this;
    }

    private static object InjectServices(IServiceProvider serviceProvider, Type concreteType)
    {
        var service = serviceProvider.GetRequiredService(concreteType);

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
