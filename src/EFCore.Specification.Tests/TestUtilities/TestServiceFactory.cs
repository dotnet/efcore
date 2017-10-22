// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
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

        public TService Create<TService>()
            where TService : class
        {
            return _factories.GetOrAdd(
                typeof(TService),
                t => AddType(new ServiceCollection(), typeof(TService)).BuildServiceProvider()).GetService<TService>();
        }

        private static ServiceCollection AddType(ServiceCollection serviceCollection, Type serviceType)
        {
            var implementationType = GetImplementationType(serviceType);

            serviceCollection.AddSingleton(serviceType, implementationType);

            var constructors = implementationType.GetConstructors();
            var constructor = constructors
                .FirstOrDefault(c => c.GetParameters().Length == constructors.Max(c2 => c2.GetParameters().Length));

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    $"Cannot use 'TestServiceFactory' for '{implementationType.ShortDisplayName()}': no public constructor.");
            }

            foreach (var parameter in constructor.GetParameters())
            {
                AddType(serviceCollection, parameter.ParameterType);
            }

            return serviceCollection;
        }

        private static Type GetImplementationType(Type serviceType)
        {
            if (!serviceType.IsInterface)
            {
                return serviceType;
            }

            var implementationTypes = serviceType
                .Assembly
                .GetTypes()
                .Where(t => serviceType.IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            if (implementationTypes.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Cannot use 'TestServiceFactory' for '{serviceType.ShortDisplayName()}': no single implementation type in same assembly.");
            }

            return implementationTypes[0];
        }
    }
}
