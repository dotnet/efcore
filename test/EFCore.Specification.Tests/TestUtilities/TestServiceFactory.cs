// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestServiceFactory
    {
        public static readonly TestServiceFactory Instance = new TestServiceFactory();

        private TestServiceFactory()
        {
        }

        private readonly ConcurrentDictionary<Type, IServiceProvider> _factories
            = new ConcurrentDictionary<Type, IServiceProvider>();

        private readonly IReadOnlyList<(Type Type, object Implementation)> _wellKnownExceptions
            = new List<(Type, object)>
            {
                (typeof(IRegisteredServices), new RegisteredServices(Enumerable.Empty<Type>())),
                (typeof(ServiceParameterBindingFactory), new ServiceParameterBindingFactory(typeof(IStateManager))),
                (typeof(IDiagnosticsLogger<DbLoggerCategory.Model>), new TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions>()),
                (typeof(IDiagnosticsLogger<DbLoggerCategory.Model.Validation>), new TestLogger<DbLoggerCategory.Model.Validation, TestLoggingDefinitions>()),
                (typeof(IDiagnosticsLogger<DbLoggerCategory.Query>), new TestLogger<DbLoggerCategory.Query, TestLoggingDefinitions>())
            };

        public TService Create<TService>(params (Type Type, object Implementation)[] specialCases)
            where TService : class
        {
            var exceptions = specialCases.Concat(_wellKnownExceptions).ToList();

            return _factories.GetOrAdd(
                typeof(TService),
                t => AddType(new ServiceCollection(), typeof(TService), exceptions).BuildServiceProvider()).GetService<TService>();
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
            => !type.GetTypeInfo().IsGenericTypeDefinition
               && type.GetTypeInfo().IsGenericType
               && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type.GetTypeInfo().GenericTypeArguments[0]
                : null;
    }
}
