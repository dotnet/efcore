// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class TestServiceFactory
{
    public static readonly TestServiceFactory Instance = new();

    private TestServiceFactory()
    {
    }

    private readonly ConcurrentDictionary<Type, IServiceProvider> _factories = new();

    private readonly IReadOnlyList<(Type Type, object Implementation)> _wellKnownExceptions
        = new List<(Type, object)>
        {
            (typeof(IRegisteredServices), new RegisteredServices(Enumerable.Empty<Type>())),
            (typeof(ServiceParameterBindingFactory), new ServiceParameterBindingFactory(typeof(IStateManager))),
            (typeof(IDiagnosticsLogger<DbLoggerCategory.Model>), new TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions>()),
            (typeof(IDiagnosticsLogger<DbLoggerCategory.Model.Validation>),
                new TestLogger<DbLoggerCategory.Model.Validation, TestLoggingDefinitions>()),
            (typeof(IDiagnosticsLogger<DbLoggerCategory.Query>), new TestLogger<DbLoggerCategory.Query, TestLoggingDefinitions>())
        };

    public TService Create<TService>(params (Type Type, object Implementation)[] specialCases)
        where TService : class
    {
        var exceptions = specialCases.Concat(_wellKnownExceptions).ToList();

        return _factories.GetOrAdd(
                typeof(TService),
                t => AddType([], typeof(TService), exceptions).BuildServiceProvider(validateScopes: true))
            .GetService<TService>();
    }

    private static ServiceCollection AddType(
        ServiceCollection serviceCollection,
        Type serviceType,
        IList<(Type Type, object Implementation)> specialCases)
    {
        var implementation = specialCases.Where(s => s.Type == serviceType).Select(s => s.Implementation).FirstOrDefault();

        if (implementation != null)
        {
            serviceCollection.AddSingleton(serviceType, implementation);
        }
        else
        {
            foreach (var (ServiceType, ImplementationType) in GetImplementationType(serviceType))
            {
                implementation = specialCases.Where(s => s.Type == ImplementationType).Select(s => s.Implementation).FirstOrDefault();

                if (implementation != null)
                {
                    serviceCollection.AddSingleton(ServiceType, implementation);
                }
                else
                {
                    serviceCollection.AddSingleton(ServiceType, ImplementationType);

                    var constructors = ImplementationType.GetConstructors();
                    var constructor = constructors
                        .FirstOrDefault(c => c.GetParameters().Length == constructors.Max(c2 => c2.GetParameters().Length));

                    if (constructor == null)
                    {
                        throw new InvalidOperationException(
                            $"Cannot use 'TestServiceFactory' for '{ImplementationType.ShortDisplayName()}': no public constructor.");
                    }

                    foreach (var parameter in constructor.GetParameters())
                    {
                        AddType(serviceCollection, parameter.ParameterType, specialCases);
                    }
                }
            }
        }

        return serviceCollection;
    }

    private static IList<(Type ServiceType, Type ImplementationType)> GetImplementationType(Type serviceType)
    {
        if (!serviceType.IsInterface)
        {
            return new[] { (serviceType, serviceType) };
        }

        var elementType = TryGetEnumerableType(serviceType);

        var implementationTypes = (elementType ?? serviceType)
            .Assembly
            .GetTypes()
            .Where(t => (elementType ?? serviceType).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        if (elementType == null)
        {
            if (implementationTypes.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Cannot use 'TestServiceFactory' for '{serviceType.ShortDisplayName()}': no single implementation type in same assembly.");
            }

            return new[] { (serviceType, implementationTypes[0]) };
        }

        return implementationTypes.Select(t => (elementType, t)).ToList();
    }

    private static Type TryGetEnumerableType(Type type)
        => !type.IsGenericTypeDefinition
            && type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type.GenericTypeArguments[0]
                : null;
}
